using NotificationGateway.Domain.Entities;

namespace NotificationGateway.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Notification?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
        Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
        Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetStuckNotificationsAsync(DateTime stuckSince, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetAbandonedNotificationsAsync(DateTime abandonedSince, CancellationToken cancellationToken = default);
        Task<int> GetStuckCountAsync(DateTime stuckSince, CancellationToken cancellationToken = default);
        Task<int> GetAbandonedCountAsync(DateTime abandonedSince, CancellationToken cancellationToken = default);

        Task<IEnumerable<Notification>> GetSentNotificationsOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetFailedNotificationsOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetPendingNotificationsOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetProcessingNotificationsOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

        Task<int> GetSentNotificationsCountOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
        Task<int> GetFailedNotificationsCountOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
        Task<int> GetPendingNotificationsCountOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
        Task<int> GetProcessingNotificationsCountOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

        Task DeleteRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    }
}
