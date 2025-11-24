namespace NotificationGateway.Infrastructure.Config
{
    public sealed class TelegramConfigSection : ConfigSection
    {
        public TelegramConfigSection() : base("Telegram") {}

        public string? BotToken { get; set; }

        public string? ApiUrl { get; set; }

        public byte TimeoutSeconds { get; set; }
    }
}
