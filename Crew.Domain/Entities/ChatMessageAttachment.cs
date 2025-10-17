using System;

namespace Crew.Domain.Entities;

public class ChatMessageAttachment
{
    public Guid Id { get; set; }
    public long MessageId { get; set; }
    public string StorageKey { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Duration { get; set; }

    public ChatMessage? Message { get; set; }
}
