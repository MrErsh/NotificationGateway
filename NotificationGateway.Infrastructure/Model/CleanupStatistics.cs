namespace NotificationGateway.Infrastructure.Model
{
    public record CleanupStatistics
    {
        public int TotalSentOlderThanRetention { get; init; }
        public int TotalFailedOlderThanRetention { get; init; }
        public int TotalPendingOlderThanRetention { get; init; }
        public int TotalStuckProcessing { get; init; }
        public TimeSpan SentRetentionPeriod { get; init; }
        public TimeSpan FailedRetentionPeriod { get; init; }
        public TimeSpan PendingRetentionPeriod { get; init; }
        public DateTime CalculatedAt { get; init; }
    }
}