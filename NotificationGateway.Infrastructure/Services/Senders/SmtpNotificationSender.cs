using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using NotificationGateway.Domain.Entities;
using NotificationGateway.Domain.Interfaces;
using NotificationGateway.Infrastructure.Config;
using NotificationGateway.Infrastructure.Exceptions;

namespace NotificationGateway.Infrastructure.Services.Senders
{
    public class SmtpNotificationSender : INotificationSender
    {
        private const int DefaultTimeoutSeconds = 30;
        private const int DefaultPort = 587;

        private readonly ILogger<SmtpNotificationSender> _logger;
        private readonly string _host;
        private readonly int _port;
        private readonly bool _useSsl;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly int _timeoutMs;

        public SmtpNotificationSender(
            IConfiguration configuration,
            ILogger<SmtpNotificationSender> logger)
        {
            _logger = logger;

            var section = configuration.GetSection<SmtpConfigSection>();

            _host = section.Host ?? throw new ConfigurationException("Smtp:Host");
            _port = section.Port ?? DefaultPort;
            _useSsl = section.EnableSsl ?? true;
            _userName = section.UserName ?? throw new ConfigurationException("Smtp:UserName");
            _password = section.Password ?? throw new ConfigurationException("Smtp:Password");
            _fromEmail = section.FromEmail ?? throw new ConfigurationException("Smtp:FromEmail");
            _fromName = section.FromName ?? _fromEmail;
            _timeoutMs = (section.TimeoutSeconds ?? DefaultTimeoutSeconds) * 1000;
        }

        public async Task SendAsync(Notification notification)
        {
            _logger.LogInformation("Sending Email notification to {Recipient}", notification.Recipient);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(MailboxAddress.Parse(notification.Recipient));
            message.Subject = notification.Subject ?? "Notification";
            message.Body = new TextPart("plain") { Text = notification.Body };

            var security = _useSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None;

            using var client = new SmtpClient { Timeout = _timeoutMs };
            await client.ConnectAsync(_host, _port, security, CancellationToken.None);
            try
            {
                if (client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
                    await client.AuthenticateAsync(_userName, _password, CancellationToken.None);

                await client.SendAsync(message, CancellationToken.None);
                await client.DisconnectAsync(true, CancellationToken.None);
            }
            catch
            {
                try { client.Disconnect(false); } catch { /* ignore disconnect errors during unwinding */ }
                throw;
            }
        }
    }
}
