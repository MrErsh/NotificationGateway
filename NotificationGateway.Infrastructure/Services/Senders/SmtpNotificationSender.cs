using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationGateway.Domain.Entities;
using NotificationGateway.Domain.Interfaces;
using NotificationGateway.Infrastructure.Config;
using NotificationGateway.Infrastructure.Exceptions;
using System.Net;
using System.Net.Mail;

namespace NotificationGateway.Infrastructure.Services.Senders
{
    public class SmtpNotificationSender : INotificationSender
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _fromEmail;
        private readonly ILogger<SmtpNotificationSender> _logger;

        public SmtpNotificationSender(
            IConfiguration configuration,
            ILogger<SmtpNotificationSender> logger)
        {
            
            _logger = logger;

            var smtpSection = configuration.GetSection<SmtpConfigSection>();
            _fromEmail = smtpSection.FromEmail ?? throw new ArgumentNullException("Smtp:FromEmail");
            var userName = smtpSection.UserName ?? throw new ConfigurationException("Smtp:UserName");
            var password = smtpSection.Password ?? throw new ConfigurationException("Smtp:Password");

            _smtpClient = new SmtpClient
            {
                Host = smtpSection?.Host ?? "localhost",
                Port = smtpSection?.Port ?? 587,
                EnableSsl = smtpSection?.EnableSsl ?? true,
                Credentials = new NetworkCredential(userName, password)
            };
        }

        public async Task SendAsync(Notification notification)
        {
            _logger.LogInformation("Sending Email notification to {Recipient}", notification.Recipient);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = notification.Subject ?? "Notification",
                Body = notification.Body,
                IsBodyHtml = false
            };
            mailMessage.To.Add(notification.Recipient);

            await _smtpClient.SendMailAsync(mailMessage);
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}
