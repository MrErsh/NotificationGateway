using FluentValidation;
using NotificationGateway.Application.DTOs;
using NotificationGateway.Domain.Enums;

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
        }

        private bool IsValidChannel(string channel)
        {
            return Enum.TryParse<NotificationChannel>(channel, out _);
        }

        private bool IsValidMessageType(string messageType)
        {
            return Enum.TryParse<MessageType>(messageType, out _);
        }
    }
}
