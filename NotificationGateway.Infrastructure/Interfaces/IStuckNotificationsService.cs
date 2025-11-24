namespace NotificationGateway.Infrastructure.Interfaces
{
    public interface IStuckNotificationsService
    {
        Task RetryStuckNotificationsAsync();
        Task<int> CleanupAbandonedNotificationsAsync();
    }
}
