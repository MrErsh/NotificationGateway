namespace NotificationGateway.Domain.ValueObjects
{
    public record IdempotencyKey
    {
        public string Value { get; }

        private IdempotencyKey(string value) => Value = value;

        public static IdempotencyKey Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Idempotency key cannot be empty", nameof(value));

            if (value.Length > 256)
                throw new ArgumentException("Idempotency key is too long", nameof(value));

            return new IdempotencyKey(value.Trim());
        }

        public override string ToString() => Value;
    }
}
