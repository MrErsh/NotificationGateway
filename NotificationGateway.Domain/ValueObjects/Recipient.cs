namespace NotificationGateway.Domain.ValueObjects
{
    public record Recipient
    {
        public string Value { get; }

        private Recipient(string value) => Value = value;

        public static Recipient Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Recipient cannot be empty", nameof(value));

            return new Recipient(value.Trim());
        }

        public override string ToString() => Value;
    }
}
