using NotificationGateway.Domain.Enums;
using NotificationGateway.Domain.Interfaces;

namespace NotificationGateway.Infrastructure.Interfaces
{
    public interface INotificationSenderFactory
    {
        INotificationSender GetSender(NotificationChannel channel);
    }
}
