using Hangfire;
using Microsoft.Extensions.Logging;
using NotificationGateway.Domain.Interfaces;
using NotificationGateway.Infrastructure.Interfaces;

namespace NotificationGateway.Infrastructure.Services
{
    public class NotificationHandler : INotificationHandler
    {
        private readonly INotificationRepository _repository;
        private readonly INotificationSenderFactory _senderFactory;
        private readonly ILogger<NotificationHandler> _logger;

        public NotificationHandler(
            INotificationRepository repository,
            INotificationSenderFactory senderFactory,
            ILogger<NotificationHandler> logger)
        {
            _repository = repository;
            _senderFactory = senderFactory;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task HandleAsync(Guid notificationId)
        {
            var notification = await _repository.GetByIdAsync(notificationId);
            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found", notificationId);
                return;
            }

            try
            {
                _logger.LogInformation(
                    "Processing notification {NotificationId} via {Channel}",
                    notificationId, notification.Channel);

                var sender = _senderFactory.GetSender(notification.Channel);
                await sender.SendAsync(notification);

                notification.MarkAsSent();
                await _repository.UpdateAsync(notification);

                _logger.LogInformation(
                    "Successfully sent notification {NotificationId} via {Channel}",
                    notificationId, notification.Channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification {NotificationId}", notificationId);

                notification.MarkAsFailed($"Send failed: {ex.Message}");
                await _repository.UpdateAsync(notification);

                throw;
            }
        }
    }
}
