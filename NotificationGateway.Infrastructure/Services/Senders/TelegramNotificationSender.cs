using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationGateway.Domain.Entities;
using NotificationGateway.Domain.Interfaces;
using NotificationGateway.Infrastructure.Config;
using NotificationGateway.Infrastructure.Exceptions;
using System.Net.Http.Json;

namespace NotificationGateway.Infrastructure.Services.Senders
{
    public class TelegramNotificationSender : INotificationSender, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TelegramNotificationSender> _logger;
        private readonly string _sendMessageUrl;

        public TelegramNotificationSender(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<TelegramNotificationSender> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Telegram");
            var tgSection = configuration.GetSection<TelegramConfigSection>();
            var apiUrl = tgSection.ApiUrl;
            if (tgSection.ApiUrl is null)
                throw new ConfigurationException("Telegram:ApiUrl");

            var botToken = tgSection.BotToken;
            if (tgSection.BotToken is null)
                throw new ConfigurationException("Telegram:BotToken");

            _sendMessageUrl = $"{apiUrl}/bot{botToken}/sendMessage";
            
            _logger = logger;
        }

        #region Implementation of INotificationSender

        public async Task SendAsync(Notification notification)
        {
            _logger.LogInformation("Sending Telegram notification to {Recipient}", notification.Recipient);

            var message = FormatTelegramMessage(notification);

            var response = await _httpClient.PostAsJsonAsync(_sendMessageUrl, new
            {
                chat_id = notification.Recipient,
                text = message,
                parse_mode = "HTML"
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Telegram API error: {response.StatusCode} - {errorContent}");
            }
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose() => _httpClient.Dispose();

        #endregion

        #region Private Methoods

        private string FormatTelegramMessage(Notification notification)
        {
            var message = $"<b>{EscapeHtml(notification.Subject ?? "Notification")}</b>\n\n";
            message += $"{EscapeHtml(notification.Body)}";

            if (!string.IsNullOrEmpty(notification.IdempotencyKey))
                message += $"\n\n<code>ID: {notification.IdempotencyKey}</code>";

            return message;
        }

        private string EscapeHtml(string text) => text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");

        #endregion
    }
}
