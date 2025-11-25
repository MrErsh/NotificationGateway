using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationGateway.Application.DTOs;
using NotificationGateway.Application.Services;
using NotificationGateway.Domain.Entities;
using NotificationGateway.Domain.Enums;
using NotificationGateway.Domain.Interfaces;
using Xunit;

namespace NotificationGateway.Application.Tests.Services
{
    public class NotificationAppServiceTests
    {
        private Mock<INotificationRepository> _repository = new();
        private Mock<INotificationProcessingService> _processingService = new();
        private Mock<IIdempotencyService> _idempotencyService = new();
        private Mock<ILogger<NotificationAppService>> _loggger = new();     

        [Fact]
        public async Task NotifyAsync_IdempotencyKeyExists_ReturnsResponseFromCache()
        {
            // Arrange
            const string key = "key1";
            var response = new NotificationResponseDto();
            _idempotencyService
                .Setup(s => s.GetResponseAsync<NotificationResponseDto>(key, default).Result)
                .Returns(response);

            var sut = CreateSut();
            var request = new NotificationRequestDto { IdempotencyKey = key };

            // Act
            var result = await sut.NotifyAsync(request, default);

            // Assert
            result.Should().Be(response);
            _idempotencyService.Verify(s => s.GetResponseAsync<NotificationResponseDto>(key, default), Times.Once);

        }

        [Fact]
        public async Task NotifyAsync_ValidRequest_ProcessNotificationAndSaveToCache()
        {
            // Arrange
            var request = new NotificationRequestDto
            {
                MessageType = Domain.Enums.MessageType.Alert.ToString(),
                Channel = Domain.Enums.NotificationChannel.Telegram.ToString(),
                Body = "body",
                IdempotencyKey = "key",
                Recipient = "recipient"
            };

            Notification? notification = null;
            _repository
                .Setup(r => r.AddAsync(It.IsAny<Notification>(), default))
                .Callback<Notification, CancellationToken>((n, _) => notification = n);

            var sut = CreateSut();

            //Act
            var result = await sut.NotifyAsync(request, default);

            //Assert
            notification.Should().NotBeNull();
            notification.Channel.Should().Equals(NotificationChannel.Telegram);
            notification.MessageType.Should().Equals(MessageType.Alert);
            notification.Body.Should().Be("body");
            notification.Recipient.Should().Be("recipient");
            notification.IdempotencyKey.Should().Be("key");
            result.Should().NotBeNull();
            result.NotificationId.Should().Equals(notification.Id);
            _repository.Verify(r => r.AddAsync(It.IsAny<Notification>(), default), Times.Once);
            _processingService.Verify(ps => ps.ProcessNotificationAsync(notification), Times.Once);
            _idempotencyService.Verify(s => s.StoreResponseAsync("key",
                                                                  It.IsAny<NotificationResponseDto>,
                                                                  TimeSpan.FromHours(24)),
                                            Times.Once);
        }

        private NotificationAppService CreateSut() =>
            new NotificationAppService(_repository.Object,
                                       _processingService.Object,
                                        _idempotencyService.Object,
                                        _loggger.Object);
        
    }
}
