namespace NotificationGateway.Infrastructure.Interfaces
{
    public interface INotificationHandler
    {
        Task HandleAsync(Guid notificationId);
    }
}
