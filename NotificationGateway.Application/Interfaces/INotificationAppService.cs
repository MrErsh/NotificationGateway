using NotificationGateway.Application.DTOs;

namespace NotificationGateway.Application.Interfaces
{
    public interface INotificationAppService
    {
        Task<NotificationResponseDto> NotifyAsync(
            NotificationRequestDto requestDto,
            CancellationToken cancellationToken = default);
    }
}
