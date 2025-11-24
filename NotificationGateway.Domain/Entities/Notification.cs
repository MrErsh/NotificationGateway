using NotificationGateway.Domain.Enums;

namespace NotificationGateway.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public MessageType MessageType { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Recipient { get; private set; }
    public string? Subject { get; private set; }
    public string Body { get; private set; }
    public NotificationStatus Status { get; private set; }
    public string? IdempotencyKey { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }
    public int AttemptsCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    private Notification() { }

    public Notification(
        MessageType messageType,
        NotificationChannel channel,
        string recipient,
        string body,
        string? subject = null,
        string? idempotencyKey = null)
    {
        Id = Guid.NewGuid();
        MessageType = messageType;
        Channel = channel;
        Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        Subject = subject;
        IdempotencyKey = idempotencyKey;
        Status = NotificationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        AttemptsCount = 0;
    }

    public void MarkAsProcessing()
    {
        Status = NotificationStatus.Processing;
        LastAttemptAt = DateTime.UtcNow;
        AttemptsCount++;
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
        LastAttemptAt = DateTime.UtcNow;
        AttemptsCount++;
    }

    public void MarkAsRetrying()
    {
        Status = NotificationStatus.Pending;
        LastAttemptAt = DateTime.UtcNow;
        AttemptsCount++;
    }
}