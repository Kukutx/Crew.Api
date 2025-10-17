using System;
using System.Collections.Generic;
using Crew.Domain.Enums;

namespace Crew.Domain.Entities;

public class Chat
{
    public Guid Id { get; set; }
    public ChatType Type { get; set; }
    public string? Title { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? EventId { get; set; }
    public bool IsArchived { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
