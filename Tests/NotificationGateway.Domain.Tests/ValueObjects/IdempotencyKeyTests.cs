using NotificationGateway.Domain.ValueObjects;
using Xunit;

namespace NotificationGateway.Domain.Tests.ValueObjects
{
    public class IdempotencyKeyTests
    {
        [Fact]
        public void Create_ValidString_ValidObject()
        {
            var record = IdempotencyKey.Create(" some_d   ");

            Assert.Equal("some_d", record.Value);
        }

        [Fact]
        public void Create_EmptyString_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => IdempotencyKey.Create("  "));
        }

        [Fact]
        public void Create_TooLongString_ThrowsException()
        {
            var value = string.Join("", Enumerable.Repeat("d", 257));

            Assert.Throws<ArgumentException>(() => IdempotencyKey.Create(value));
        }
    }
}
