using System;
using System.Collections.Generic;
using Crew.Domain.Enums;

namespace Crew.Domain.Entities;

public class ChatMessage
{
    public long Id { get; set; }
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public ChatMessageKind Kind { get; set; }
    public string? BodyText { get; set; }
    public string? MetaJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long Seq { get; set; }
    public ChatMessageStatus Status { get; set; }

    public Chat? Chat { get; set; }
    public User? Sender { get; set; }
    public ICollection<ChatMessageAttachment> Attachments { get; set; } = new List<ChatMessageAttachment>();
    public ICollection<ChatMessageReaction> Reactions { get; set; } = new List<ChatMessageReaction>();
}
