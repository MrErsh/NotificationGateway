using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationGateway.Domain.Entities;
using NotificationGateway.Domain.Interfaces;
using NotificationGateway.Infrastructure.Config;
using NotificationGateway.Infrastructure.Interfaces;
using NotificationGateway.Infrastructure.Model;

namespace NotificationGateway.Infrastructure.Services
{
    // TODO VE: тестов бы
    public class NotificationCleanupService : INotificationCleanupService
    {
        private readonly INotificationRepository _repository;
        private readonly ILogger<NotificationCleanupService> _logger;
        private readonly TimeSpan _sentRetentionPeriod;
        private readonly TimeSpan _failedRetentionPeriod;
        private readonly TimeSpan _pendingRetentionPeriod;
        private readonly TimeSpan _processingThreshold;

        public NotificationCleanupService(
            INotificationRepository repository,
            IConfiguration configuration,
            ILogger<NotificationCleanupService> logger)
        {
            _repository = repository;
            _logger = logger;

            var retentionSection = configuration.GetSection<RetentionSection>();

            
            _sentRetentionPeriod = TimeSpan.FromDays(
                retentionSection.SentNotificationsDays ?? 30);
            _failedRetentionPeriod = TimeSpan.FromDays(
                retentionSection.FailedNotificationsDays ?? 7);
            _pendingRetentionPeriod = TimeSpan.FromDays(
                retentionSection.PendingNotificationsDays ?? 1);
            _processingThreshold = TimeSpan.FromMinutes(
                retentionSection.StuckProcessingMinutes ?? 30);
        }

        public async Task<int> CleanupOldNotificationsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting comprehensive cleanup of old notifications...");

                var totalCleaned = 0;
                var stats = await GetCleanupStatisticsAsync(cancellationToken);

                // Очищаем отправленные уведомления
                if (stats.TotalSentOlderThanRetention > 0)
                {
                    var sentCleaned = await CleanupSentNotificationsAsync(_sentRetentionPeriod, cancellationToken);
                    totalCleaned += sentCleaned;
                    _logger.LogInformation("Cleaned {Count} sent notifications", sentCleaned);
                }

                // Очищаем проваленные уведомления
                if (stats.TotalFailedOlderThanRetention > 0)
                {
                    var failedCleaned = await CleanupFailedNotificationsAsync(_failedRetentionPeriod, cancellationToken);
                    totalCleaned += failedCleaned;
                    _logger.LogInformation("Cleaned {Count} failed notifications", failedCleaned);
                }

                // Очищаем зависшие pending уведомления
                if (stats.TotalPendingOlderThanRetention > 0)
                {
                    var pendingCleaned = await CleanupPendingNotificationsAsync(_pendingRetentionPeriod, cancellationToken);
                    totalCleaned += pendingCleaned;
                    _logger.LogInformation("Cleaned {Count} pending notifications", pendingCleaned);
                }

                _logger.LogInformation(
                    "Comprehensive cleanup completed. Total notifications cleaned: {TotalCleaned}",
                    totalCleaned);

                return totalCleaned;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during comprehensive notification cleanup");
                throw;
            }
        }

        public async Task<int> CleanupSentNotificationsAsync(TimeSpan olderThan, CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Subtract(olderThan);
                _logger.LogDebug("Cleaning up sent notifications older than {CutoffDate}", cutoffDate);

                var sentNotifications = await _repository.GetSentNotificationsOlderThanAsync(cutoffDate, cancellationToken);
                var notificationsToDelete = sentNotifications.ToList();

                if (!notificationsToDelete.Any())
                {
                    _logger.LogDebug("No sent notifications found older than {CutoffDate}", cutoffDate);
                    return 0;
                }

                await _repository.DeleteRangeAsync(notificationsToDelete, cancellationToken);

                _logger.LogInformation(
                    "Successfully cleaned up {Count} sent notifications older than {CutoffDate}",
                    notificationsToDelete.Count, cutoffDate);

                return notificationsToDelete.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up sent notifications older than {OlderThan}", olderThan);
                throw;
            }
        }

        public async Task<int> CleanupFailedNotificationsAsync(TimeSpan olderThan, CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Subtract(olderThan);
                _logger.LogDebug("Cleaning up failed notifications older than {CutoffDate}", cutoffDate);

                var failedNotifications = await _repository.GetFailedNotificationsOlderThanAsync(cutoffDate, cancellationToken);
                var notificationsToDelete = failedNotifications.ToList();

                if (!notificationsToDelete.Any())
                {
                    _logger.LogDebug("No failed notifications found older than {CutoffDate}", cutoffDate);
                    return 0;
                }

                // Логируем информацию о проваленных уведомлениях перед удалением
                foreach (var notification in notificationsToDelete)
                {
                    _logger.LogWarning(
                        "Cleaning up failed notification {NotificationId}. Attempts: {Attempts}, Last Error: {Error}",
                        notification.Id, notification.AttemptsCount, notification.ErrorMessage);
                }

                await _repository.DeleteRangeAsync(notificationsToDelete, cancellationToken);

                _logger.LogInformation(
                    "Successfully cleaned up {Count} failed notifications older than {CutoffDate}",
                    notificationsToDelete.Count, cutoffDate);

                return notificationsToDelete.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up failed notifications older than {OlderThan}", olderThan);
                throw;
            }
        }

        public async Task<int> CleanupPendingNotificationsAsync(TimeSpan olderThan, CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Subtract(olderThan);
                _logger.LogDebug("Cleaning up pending notifications older than {CutoffDate}", cutoffDate);

                var pendingNotifications = await _repository.GetPendingNotificationsOlderThanAsync(cutoffDate, cancellationToken);
                var notificationsToDelete = pendingNotifications.ToList();

                if (!notificationsToDelete.Any())
                {
                    _logger.LogDebug("No pending notifications found older than {CutoffDate}", cutoffDate);
                    return 0;
                }

                // Логируем информацию о зависших pending уведомлениях
                foreach (var notification in notificationsToDelete)
                {
                    _logger.LogWarning(
                        "Cleaning up stuck pending notification {NotificationId}. Created: {CreatedAt}",
                        notification.Id, notification.CreatedAt);
                }

                await _repository.DeleteRangeAsync(notificationsToDelete, cancellationToken);

                _logger.LogInformation(
                    "Successfully cleaned up {Count} pending notifications older than {CutoffDate}",
                    notificationsToDelete.Count, cutoffDate);

                return notificationsToDelete.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up pending notifications older than {OlderThan}", olderThan);
                throw;
            }
        }

        public async Task<CleanupStatistics> GetCleanupStatisticsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var now = DateTime.UtcNow;
                var sentCutoff = now.Subtract(_sentRetentionPeriod);
                var failedCutoff = now.Subtract(_failedRetentionPeriod);
                var pendingCutoff = now.Subtract(_pendingRetentionPeriod);
                var stuckProcessingCutoff = now.Subtract(_processingThreshold);

                var sentCount = await _repository.GetSentNotificationsCountOlderThanAsync(sentCutoff, cancellationToken);
                var failedCount = await _repository.GetFailedNotificationsCountOlderThanAsync(failedCutoff, cancellationToken);
                var pendingCount = await _repository.GetPendingNotificationsCountOlderThanAsync(pendingCutoff, cancellationToken);
                var stuckProcessingCount = await _repository.GetProcessingNotificationsCountOlderThanAsync(stuckProcessingCutoff, cancellationToken);

                return new CleanupStatistics
                {
                    TotalSentOlderThanRetention = sentCount,
                    TotalFailedOlderThanRetention = failedCount,
                    TotalPendingOlderThanRetention = pendingCount,
                    TotalStuckProcessing = stuckProcessingCount,
                    SentRetentionPeriod = _sentRetentionPeriod,
                    FailedRetentionPeriod = _failedRetentionPeriod,
                    PendingRetentionPeriod = _pendingRetentionPeriod,
                    CalculatedAt = now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cleanup statistics");
                throw;
            }
        }

        public async Task<int> ArchiveNotificationsAsync(IEnumerable<Guid> notificationIds, CancellationToken cancellationToken = default)
        {
            try
            {
                var ids = notificationIds.ToList();
                if (!ids.Any())
                {
                    _logger.LogWarning("No notification IDs provided for archiving");
                    return 0;
                }

                _logger.LogInformation("Starting archive process for {Count} notifications", ids.Count);

                var archivedCount = 0;
                var notificationsToArchive = new List<Notification>();

                // Получаем уведомления для архивации
                foreach (var id in ids)
                {
                    var notification = await _repository.GetByIdAsync(id, cancellationToken);
                    if (notification != null)
                    {
                        notificationsToArchive.Add(notification);
                    }
                }

                if (!notificationsToArchive.Any())
                {
                    _logger.LogWarning("No notifications found for archiving");
                    return 0;
                }

                // По хорошему надо бы сделать так:
                // 1. Создание архивных записей в отдельной таблице
                // 2. Удаление оригинальных записей
                // 3. Логирование процесса

                // Временная реализация - просто удаляем
                await _repository.DeleteRangeAsync(notificationsToArchive, cancellationToken);
                archivedCount = notificationsToArchive.Count;

                _logger.LogInformation(
                    "Successfully archived {Count} notifications. Archived IDs: {NotificationIds}",
                    archivedCount, string.Join(", ", notificationsToArchive.Select(n => n.Id)));

                return archivedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving notifications");
                throw;
            }
        }

        public async Task<string> GetCleanupReportAsync(CancellationToken cancellationToken = default)
        {
            var stats = await GetCleanupStatisticsAsync(cancellationToken);

            return $"""
            Notification Cleanup Report ({stats.CalculatedAt:yyyy-MM-dd HH:mm:ss})
            =============================================
            Sent notifications older than {stats.SentRetentionPeriod.Days} days: {stats.TotalSentOlderThanRetention}
            Failed notifications older than {stats.FailedRetentionPeriod.Days} days: {stats.TotalFailedOlderThanRetention}
            Pending notifications older than {stats.PendingRetentionPeriod.Days} days: {stats.TotalPendingOlderThanRetention}
            Stuck processing notifications: {stats.TotalStuckProcessing}
            =============================================
            Total eligible for cleanup: {stats.TotalSentOlderThanRetention + stats.TotalFailedOlderThanRetention + stats.TotalPendingOlderThanRetention}
            """;
        }
    }
}
