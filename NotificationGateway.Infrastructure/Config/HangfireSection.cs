namespace NotificationGateway.Infrastructure.Config
{
    public sealed class HangfireSection : ConfigSection
    {
        public HangfireSection() : base("Hangfire") { }

        public int? WorkerCount { get; set; }

        public string[]? Queues { get; set; }
    }
}
