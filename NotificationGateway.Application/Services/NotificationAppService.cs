using Microsoft.Extensions.Logging;
using NotificationGateway.Application.DTOs;
using NotificationGateway.Application.Interfaces;
using NotificationGateway.Domain.Entities;
using NotificationGateway.Domain.Enums;
using NotificationGateway.Domain.Exceptions;
using NotificationGateway.Domain.Interfaces;

namespace NotificationGateway.Application.Services
{
    public class NotificationAppService : INotificationAppService
    {
        private static readonly TimeSpan IdempotencyCacheTtl = TimeSpan.FromHours(24);

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
            var hasKey = !string.IsNullOrEmpty(requestDto.IdempotencyKey);

            if (hasKey)
            {
                var cachedResponse = await _idempotencyService.GetResponseAsync<NotificationResponseDto>(
                    requestDto.IdempotencyKey!, cancellationToken);

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

            try
            {
                await _repository.AddAsync(notification, cancellationToken);
            }
            catch (DuplicateIdempotencyKeyException ex) when (hasKey)
            {
                var winner = await _repository.GetByIdempotencyKeyAsync(ex.IdempotencyKey, cancellationToken)
                             ?? throw new InvalidOperationException(
                                 $"Duplicate idempotency key '{ex.IdempotencyKey}' reported by the store, but the winner row was not found.");

                var winnerResponse = ToResponseDto(winner);
                await _idempotencyService.StoreResponseAsync(ex.IdempotencyKey, winnerResponse, IdempotencyCacheTtl);

                _logger.LogInformation(
                    "Idempotency race resolved: key {Key} handled by existing notification {NotificationId}",
                    ex.IdempotencyKey, winner.Id);
                return winnerResponse;
            }

            await _processingService.ProcessNotificationAsync(notification, cancellationToken);

            var responseDto = ToResponseDto(notification);

            if (hasKey)
                await _idempotencyService.StoreResponseAsync(requestDto.IdempotencyKey!, responseDto, IdempotencyCacheTtl);

            _logger.LogInformation("Created notification {NotificationId}", notification.Id);
            return responseDto;
        }

        private static NotificationResponseDto ToResponseDto(Notification notification) => new()
        {
            NotificationId = notification.Id,
            Status = notification.Status.ToString(),
            CreatedAt = notification.CreatedAt
        };
    }
}
