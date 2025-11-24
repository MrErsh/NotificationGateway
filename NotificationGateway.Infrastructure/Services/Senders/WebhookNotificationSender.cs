using Microsoft.Extensions.Logging;
using NotificationGateway.Domain.Entities;
using NotificationGateway.Domain.Interfaces;
using System.Net.Http.Json;

namespace NotificationGateway.Infrastructure.Services.Senders
{
    public class WebhookNotificationSender : INotificationSender, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WebhookNotificationSender> _logger;

        public WebhookNotificationSender(
            IHttpClientFactory httpClientFactory,
            ILogger<WebhookNotificationSender> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Webhook");
            _logger = logger;
        }

        public async Task SendAsync(Notification notification)
        {
            _logger.LogInformation("Sending Webhook notification to {Recipient}", notification.Recipient);

            var webhookPayload = new
            {
                notificationId = notification.Id,
                messageType = notification.MessageType.ToString(),
                recipient = notification.Recipient,
                subject = notification.Subject,
                body = notification.Body,
                timestamp = notification.CreatedAt,
                idempotencyKey = notification.IdempotencyKey
            };

            var response = await _httpClient.PostAsJsonAsync(notification.Recipient, webhookPayload);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Webhook error: {response.StatusCode} - {errorContent}");
            }
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
