using Hangfire;
using Microsoft.Extensions.Logging;
using NotificationGateway.Domain.Interfaces;
using NotificationGateway.Infrastructure.Interfaces;

namespace NotificationGateway.Infrastructure.Services
{
    public class StuckNotificationsService : IStuckNotificationsService
    {
        private readonly INotificationRepository _repository;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<StuckNotificationsService> _logger;
        private readonly TimeSpan _stuckThreshold = TimeSpan.FromMinutes(30);
        private readonly TimeSpan _abandonedThreshold = TimeSpan.FromDays(7);

        public StuckNotificationsService(
            INotificationRepository repository,
            IBackgroundJobClient backgroundJobClient,
            ILogger<StuckNotificationsService> logger)
        {
            _repository = repository;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        public async Task RetryStuckNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Checking for stuck notifications...");

                var stuckThreshold = DateTime.UtcNow.Subtract(_stuckThreshold);
                var stuckNotifications = await _repository.GetStuckNotificationsAsync(stuckThreshold);

                _logger.LogInformation("Found {Count} stuck notifications", stuckNotifications.Count());

                foreach (var notification in stuckNotifications)
                {
                    try
                    {
                        _logger.LogWarning(
                            "Retrying stuck notification {NotificationId} (stuck since {LastAttempt})",
                            notification.Id, notification.LastAttemptAt);

                        notification.MarkAsRetrying();
                        await _repository.UpdateAsync(notification);

                        var jobId = _backgroundJobClient.Enqueue<INotificationHandler>(
                            handler => handler.HandleAsync(notification.Id));

                        _logger.LogInformation(
                            "Re-queued stuck notification {NotificationId}. New JobId: {JobId}",
                            notification.Id, jobId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to retry stuck notification {NotificationId}",
                            notification.Id);
                    }
                }

                _logger.LogInformation("Completed retrying stuck notifications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing stuck notifications");
                throw;
            }
        }

        public async Task<int> CleanupAbandonedNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Cleaning up abandoned notifications...");

                var abandonedThreshold = DateTime.UtcNow.Subtract(_abandonedThreshold);
                var abandonedNotifications = await _repository.GetAbandonedNotificationsAsync(abandonedThreshold);

                var cleanupCount = 0;

                foreach (var notification in abandonedNotifications)
                {
                    try
                    {
                        // TODO VE: Здесь логика очистки - можно удалить или пометить архивными
                        cleanupCount++;

                        _logger.LogInformation(
                            "Cleaned up abandoned notification {NotificationId} from {CreatedAt}",
                            notification.Id, notification.CreatedAt);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to clean up abandoned notification {NotificationId}",
                            notification.Id);
                    }
                }

                _logger.LogInformation("Cleaned up {Count} abandoned notifications", cleanupCount);
                return cleanupCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cleaning up abandoned notifications");
                throw;
            }
        }
    }
}
