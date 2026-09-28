using FluentValidation;
using NotificationGateway.Application.DTOs;
using NotificationGateway.Domain.Enums;
using NotificationGateway.Domain.Security;

namespace NotificationGateway.Application.Validators
{
    public class NotificationRequestValidator : AbstractValidator<NotificationRequestDto>
    {
        public NotificationRequestValidator()
        {
            RuleFor(x => x.Recipient)
                .NotEmpty().WithMessage("Recipient is required")
                .MaximumLength(500).WithMessage("Recipient is too long");

            RuleFor(x => x.Body)
                .NotEmpty().WithMessage("Body is required")
                .MaximumLength(4000).WithMessage("Body is too long");

            RuleFor(x => x.Subject)
                .MaximumLength(1000).WithMessage("Subject is too long")
                .When(x => !string.IsNullOrEmpty(x.Subject));

            RuleFor(x => x.Channel)
                .Must(IsValidChannel).WithMessage("Invalid channel");

            RuleFor(x => x.MessageType)
                .Must(IsValidMessageType).WithMessage("Invalid message type");

            RuleFor(x => x.Recipient)
                .Must(UrlSafety.IsSafeWebhookTarget)
                .WithMessage("Webhook recipient must be a public http/https URL")
                .When(x => IsWebhookChannel(x.Channel));
        }

        private static bool IsValidChannel(string channel)
            => Enum.TryParse<NotificationChannel>(channel, out _);

        private static bool IsValidMessageType(string messageType)
            => Enum.TryParse<MessageType>(messageType, out _);

        private static bool IsWebhookChannel(string channel)
            => string.Equals(channel, nameof(NotificationChannel.Webhook), StringComparison.OrdinalIgnoreCase);
    }
}
