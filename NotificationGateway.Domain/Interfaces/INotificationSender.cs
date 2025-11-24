using NotificationGateway.Domain.Entities;

namespace NotificationGateway.Domain.Interfaces
{
    public interface INotificationSender
    {
        Task SendAsync(Notification notification);
    }
}
