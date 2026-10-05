using Microsoft.Extensions.Logging;
using NotificationGateway.Domain.Entities;
using NotificationGateway.Domain.Interfaces;
using NotificationGateway.Domain.Security;
using System.Net;
using System.Net.Http.Json;

namespace NotificationGateway.Infrastructure.Services.Senders
{
    public class WebhookNotificationSender : INotificationSender
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
            if (!Uri.TryCreate(notification.Recipient, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException(
                    $"Webhook recipient is not a valid http/https URL: {notification.Recipient}");
            }

            await EnsureNotPointingToPrivateNetwork(uri);

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

        private async Task EnsureNotPointingToPrivateNetwork(Uri uri)
        {
            if (UrlSafety.IsIpAddressLiteral(uri.Host))
                return;

            IPAddress[] addresses;
            try
            {
                addresses = await Dns.GetHostAddressesAsync(uri.Host);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Webhook recipient host could not be resolved: {uri.Host}", ex);
            }

            foreach (var address in addresses)
            {
                if (UrlSafety.IsPrivateAddress(address))
                {
                    _logger.LogWarning(
                        "Blocked SSRF attempt: {Host} resolved to {Address}",
                        uri.Host, address);
                    throw new InvalidOperationException(
                        $"Webhook recipient resolves to a private/internal address: {uri.Host}");
                }
            }
        }
    }
}
