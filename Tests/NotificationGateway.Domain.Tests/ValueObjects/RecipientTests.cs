using NotificationGateway.Domain.ValueObjects;
using Xunit;

namespace NotificationGateway.Domain.Tests.ValueObjects
{
    public class RecipientTests
    {
        [Fact]
        public void Create_ValidString_ValidObject()
        {
            var record = Recipient.Create(" some_d   ");

            Assert.Equal("some_d", record.Value);
        }

        [Fact]
        public void Create_EmptyString_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Recipient.Create("  "));
        }
    }
}
