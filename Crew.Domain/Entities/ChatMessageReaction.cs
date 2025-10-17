using System;

namespace Crew.Domain.Entities;

public class ChatMessageReaction
{
    public long MessageId { get; set; }
    public Guid UserId { get; set; }
    public string Emoji { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public ChatMessage? Message { get; set; }
    public User? User { get; set; }
}
