namespace Crew.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = null!;
    public string? AttachmentsJson { get; set; }
    public DateTimeOffset SentAt { get; set; }
    public ChatGroup? Group { get; set; }
    public User? Sender { get; set; }
}
