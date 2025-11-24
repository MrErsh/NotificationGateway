using Microsoft.Extensions.DependencyInjection;
using NotificationGateway.Domain.Enums;
using NotificationGateway.Domain.Interfaces;
using NotificationGateway.Infrastructure.Interfaces;
using NotificationGateway.Infrastructure.Services.Senders;

namespace NotificationGateway.Infrastructure.Services
{
    public class NotificationSenderFactory : INotificationSenderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public NotificationSenderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public INotificationSender GetSender(NotificationChannel channel)
        {
            return channel switch
            {
                NotificationChannel.Telegram => _serviceProvider.GetRequiredService<TelegramNotificationSender>(),
                NotificationChannel.Email => _serviceProvider.GetRequiredService<SmtpNotificationSender>(),
                NotificationChannel.Webhook => _serviceProvider.GetRequiredService<WebhookNotificationSender>(),
                _ => throw new ArgumentException($"Unsupported channel: {channel}")
            };
        }
    }
}
