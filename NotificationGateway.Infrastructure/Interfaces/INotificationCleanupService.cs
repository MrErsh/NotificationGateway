using NotificationGateway.Infrastructure.Model;

namespace NotificationGateway.Infrastructure.Interfaces
{
    public interface INotificationCleanupService
    {
        Task<int> CleanupOldNotificationsAsync(CancellationToken cancellationToken = default);
        Task<int> CleanupSentNotificationsAsync(TimeSpan olderThan, CancellationToken cancellationToken = default);
        Task<int> CleanupFailedNotificationsAsync(TimeSpan olderThan, CancellationToken cancellationToken = default);
        Task<int> CleanupPendingNotificationsAsync(TimeSpan olderThan, CancellationToken cancellationToken = default);
        Task<CleanupStatistics> GetCleanupStatisticsAsync(CancellationToken cancellationToken = default);
        Task<int> ArchiveNotificationsAsync(IEnumerable<Guid> notificationIds, CancellationToken cancellationToken = default);
    }
}
