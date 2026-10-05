namespace NotificationGateway.Infrastructure.Config
{
    public sealed class SmtpConfigSection : ConfigSection
    {
        public SmtpConfigSection() : base("Smtp") { }

        public string? Host { get; set; }

        public ushort? Port { get; set; }

        public bool? EnableSsl { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public string? FromEmail { get; set; }

        public string? FromName { get; set; }

        public byte? TimeoutSeconds { get; set; }

    }
}
