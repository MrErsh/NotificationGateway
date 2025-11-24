using Hangfire;
using Microsoft.Extensions.Logging;
using NotificationGateway.Domain.Entities;
using NotificationGateway.Domain.Interfaces;
using NotificationGateway.Infrastructure.Interfaces;

namespace NotificationGateway.Infrastructure.Services
{
    public class NotificationProcessingService : INotificationProcessingService
    {
        private readonly INotificationRepository _repository;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<NotificationProcessingService> _logger;

        public NotificationProcessingService(
            INotificationRepository repository,
            IBackgroundJobClient backgroundJobClient,
            ILogger<NotificationProcessingService> logger)
        {
            _repository = repository;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        public async Task<Guid> ProcessNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            notification.MarkAsProcessing();
            await _repository.UpdateAsync(notification, cancellationToken);

            var jobId = _backgroundJobClient.Enqueue<INotificationHandler>(
                handler => handler.HandleAsync(notification.Id));

            _logger.LogInformation(
                "Enqueued notification {NotificationId} for processing via {Channel}. JobId: {JobId}",
                notification.Id, notification.Channel, jobId);

            return notification.Id;
        }
    }
}
