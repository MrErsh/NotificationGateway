using Microsoft.Extensions.Logging;
using NotificationGateway.Application.DTOs;
using NotificationGateway.Application.Interfaces;
using NotificationGateway.Domain.Entities;
using NotificationGateway.Domain.Enums;
using NotificationGateway.Domain.Interfaces;

namespace NotificationGateway.Application.Services
{
    public class NotificationAppService : INotificationAppService
    {
        private readonly INotificationRepository _repository;
        private readonly INotificationProcessingService _processingService;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ILogger<NotificationAppService> _logger;

        public NotificationAppService(
            INotificationRepository repository,
            INotificationProcessingService processingService,
            IIdempotencyService idempotencyService,
            ILogger<NotificationAppService> logger)
        {
            _repository = repository;
            _processingService = processingService;
            _idempotencyService = idempotencyService;
            _logger = logger;
        }

        public async Task<NotificationResponseDto> NotifyAsync(
            NotificationRequestDto requestDto,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(requestDto.IdempotencyKey))
            {
                var cachedResponse = await _idempotencyService.GetResponseAsync<NotificationResponseDto>(
                    requestDto.IdempotencyKey, cancellationToken);

                if (cachedResponse != null)
                {
                    _logger.LogInformation("Returning cached response for idempotency key: {Key}", requestDto.IdempotencyKey);
                    return cachedResponse;
                }
            }

            var messageType = Enum.Parse<MessageType>(requestDto.MessageType);
            var channel = Enum.Parse<NotificationChannel>(requestDto.Channel);

            var notification = new Notification(
                messageType,
                channel,
                requestDto.Recipient,
                requestDto.Body,
                requestDto.Subject,
                requestDto.IdempotencyKey);

            await _repository.AddAsync(notification, cancellationToken);
            await _processingService.ProcessNotificationAsync(notification, cancellationToken);

            var responseDto = new NotificationResponseDto
            {
                NotificationId = notification.Id,
                Status = notification.Status.ToString(),
                CreatedAt = notification.CreatedAt
            };

            if (!string.IsNullOrEmpty(requestDto.IdempotencyKey))
            {
                await _idempotencyService.StoreResponseAsync(
                    requestDto.IdempotencyKey,
                    responseDto,
                    TimeSpan.FromHours(24));
            }

            _logger.LogInformation("Created notification {NotificationId}", notification.Id);
            return responseDto;
        }
    }
}
