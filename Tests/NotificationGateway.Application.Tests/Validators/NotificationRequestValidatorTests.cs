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

        [Fact]
        public void Validate_WebhookChannelWithPublicHttpsUrl_True()
        {
            var notification = CreateInstance(n =>
            {
                n.Channel = "Webhook";
                n.Recipient = "https://example.com/hooks/x";
            });

            var result = Validator.Validate(notification);

            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("http://localhost/x")]
        [InlineData("http://127.0.0.1/x")]
        [InlineData("http://169.254.169.254/latest/meta-data/")]
        [InlineData("http://10.0.0.5/x")]
        [InlineData("http://192.168.1.1/x")]
        [InlineData("file:///etc/passwd")]
        [InlineData("not a url")]
        public void Validate_WebhookChannelWithUnsafeRecipient_False(string recipient)
        {
            var notification = CreateInstance(n =>
            {
                n.Channel = "Webhook";
                n.Recipient = recipient;
            });

            var result = Validator.Validate(notification);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_NonWebhookChannelWithNonUrlRecipient_True()
        {
            var notification = CreateInstance(n =>
            {
                n.Channel = "Telegram";
                n.Recipient = "12345678";
            });

            var result = Validator.Validate(notification);

            result.IsValid.Should().BeTrue();
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
