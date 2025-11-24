using FluentAssertions;
using NotificationGateway.Application.DTOs;
using NotificationGateway.Application.Validators;
using Xunit;

namespace NotificationGateway.Application.Tests.Validators
{
    public class NotificationRequestValidatorTests
    {
        private NotificationRequestValidator Validator => new();
        
        [Fact]
        public void Validate_ValidRequest_True()
        {
            var notification = CreateInstance();

            var result = Validator.Validate(notification);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_RecipientIsEmpty_False()
        {
            var notification = CreateInstance(n => n.Recipient = "");

            var result = Validator.Validate(notification);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_TooLongRecipient_False()
        {
            var notification = CreateInstance(n => n.Recipient = GenerateString(501));

            var result = Validator.Validate(notification);
            
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_BodyIsEmpty_False()
        {
            var notification = CreateInstance(n => n.Body = "");

            var result = Validator.Validate(notification);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_BodyTooLong_False()
        {
            var notification = CreateInstance(n => n.Body = GenerateString(4001));

            var result = Validator.Validate(notification);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_SubjectIsTooLong()
        {
            var notification = CreateInstance(n => n.Subject = GenerateString(1001));

            var result = Validator.Validate(notification);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_InvalidChannel_False()
        {
            var notification = CreateInstance(n => n.Channel = "Telegram1");

            var result = Validator.Validate(notification);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_InvalidMessageType_False()
        {
            var notification = CreateInstance(n => n.MessageType = "Alert1");

            var result = Validator.Validate(notification);

            result.IsValid.Should().BeFalse();
        }

        private string GenerateString(int length)       
            => string.Join("", Enumerable.Repeat("1", length));

        private NotificationRequestDto CreateInstance(Action<NotificationRequestDto> modifyAction = null)
        {
            var request = new NotificationRequestDto
            {
                Body = "Body",
                Recipient = "recipient",
                Subject = "subject",
                Channel = "Telegram",
                MessageType = "Alert"
            };

            modifyAction?.Invoke(request);

            return request;
        }
    }
}
