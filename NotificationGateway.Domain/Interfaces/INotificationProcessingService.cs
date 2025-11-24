using NotificationGateway.Domain.Entities;

namespace NotificationGateway.Domain.Interfaces
{
    public interface INotificationProcessingService
    {
        Task<Guid> ProcessNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
    }
}
