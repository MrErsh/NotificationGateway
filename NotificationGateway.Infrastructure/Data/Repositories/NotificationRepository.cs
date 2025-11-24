using Microsoft.EntityFrameworkCore;
using NotificationGateway.Domain.Entities;
using NotificationGateway.Domain.Enums;
using NotificationGateway.Domain.Interfaces;

namespace NotificationGateway.Infrastructure.Data.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }

        public async Task<Notification?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.IdempotencyKey == idempotencyKey, cancellationToken);
        }

        public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            await _context.Notifications.AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsWithIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .AnyAsync(n => n.IdempotencyKey == idempotencyKey, cancellationToken);
        }

        public async Task<IEnumerable<Notification>> GetStuckNotificationsAsync(
            DateTime stuckSince,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Processing
                            && n.LastAttemptAt < stuckSince)
                .OrderBy(n => n.LastAttemptAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Notification>> GetAbandonedNotificationsAsync(
            DateTime abandonedSince,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Failed
                            && n.LastAttemptAt < abandonedSince 
                            && n.AttemptsCount >= 3)
                .OrderBy(n => n.LastAttemptAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetStuckCountAsync(
            DateTime stuckSince,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .CountAsync(n => n.Status == NotificationStatus.Processing
                                 && n.LastAttemptAt < stuckSince,
                           cancellationToken);
        }

        public async Task<int> GetAbandonedCountAsync(
            DateTime abandonedSince,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .CountAsync(n => n.Status == NotificationStatus.Failed
                                 && n.LastAttemptAt < abandonedSince
                                 && n.AttemptsCount >= 3,
                           cancellationToken);
        }

        public async Task<IEnumerable<Notification>> GetSentNotificationsOlderThanAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Sent &&
                           n.CreatedAt < cutoffDate)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Notification>> GetFailedNotificationsOlderThanAsync(
            DateTime cutoffDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Failed &&
                           n.CreatedAt < cutoffDate)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Notification>> GetPendingNotificationsOlderThanAsync(
            DateTime cutoffDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Pending &&
                           n.CreatedAt < cutoffDate)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Notification>> GetProcessingNotificationsOlderThanAsync(
            DateTime cutoffDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Processing &&
                           n.LastAttemptAt < cutoffDate)
                .OrderBy(n => n.LastAttemptAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetSentNotificationsCountOlderThanAsync(
            DateTime cutoffDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .CountAsync(n => n.Status == NotificationStatus.Sent &&
                                n.CreatedAt < cutoffDate,
                           cancellationToken);
        }

        public async Task<int> GetFailedNotificationsCountOlderThanAsync(
            DateTime cutoffDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .CountAsync(n => n.Status == NotificationStatus.Failed &&
                                n.CreatedAt < cutoffDate,
                           cancellationToken);
        }

        public async Task<int> GetPendingNotificationsCountOlderThanAsync(
            DateTime cutoffDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .CountAsync(n => n.Status == NotificationStatus.Pending &&
                                n.CreatedAt < cutoffDate,
                           cancellationToken);
        }

        public async Task<int> GetProcessingNotificationsCountOlderThanAsync(
            DateTime cutoffDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .CountAsync(n => n.Status == NotificationStatus.Processing &&
                                n.LastAttemptAt < cutoffDate,
                           cancellationToken);
        }

        public async Task DeleteRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
        {
            var notificationsList = notifications.ToList();
            if (!notificationsList.Any())
                return;

            _context.Notifications.RemoveRange(notificationsList);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
