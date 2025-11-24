using Microsoft.Extensions.Logging;
using NotificationGateway.Domain.Entities;
using NotificationGateway.Domain.Interfaces;
using NotificationGateway.Infrastructure.Interfaces;

namespace NotificationGateway.Infrastructure.Services
{
    public class NotificationSenderOrchestrator : INotificationSender
    {
        private readonly INotificationSenderFactory _senderFactory;
        private readonly ILogger<NotificationSenderOrchestrator> _logger;

        public NotificationSenderOrchestrator(
            INotificationSenderFactory senderFactory,
            ILogger<NotificationSenderOrchestrator> logger)
        {
            _senderFactory = senderFactory;
            _logger = logger;
        }

        public async Task SendAsync(Notification notification)
        {
            try
            {
                _logger.LogInformation(
                    "Orchestrating notification {NotificationId} via {Channel} to {Recipient}",
                    notification.Id, notification.Channel, notification.Recipient);

                var sender = _senderFactory.GetSender(notification.Channel);

                await sender.SendAsync(notification);

                _logger.LogInformation(
                    "Successfully orchestrated notification {NotificationId} via {Channel}",
                    notification.Id, notification.Channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to orchestrate notification {NotificationId} via {Channel}",
                    notification.Id, notification.Channel);
                throw;
            }
        }
    }
}