namespace NotificationGateway.Infrastructure.Config
{
    public class RetentionSection : ConfigSection
    {
        public RetentionSection() : base("Retention") { }
        public byte? SentNotificationsDays { get; set; }

        public byte? FailedNotificationsDays { get; set; }

        public byte? PendingNotificationsDays { get; set; }

        public byte? StuckProcessingMinutes { get; set; }
    }
}