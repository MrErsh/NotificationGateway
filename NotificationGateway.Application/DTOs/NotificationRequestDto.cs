namespace NotificationGateway.Application.DTOs
{
    public class NotificationRequestDto
    {
        public string MessageType { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
        public string? IdempotencyKey { get; set; }
    }
}
