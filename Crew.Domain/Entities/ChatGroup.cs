using Crew.Domain.Enums;

namespace Crew.Domain.Entities;

public class ChatGroup
{
    public Guid Id { get; set; }
    public GroupScope Scope { get; set; }
    public Guid? EventId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<ChatMembership> Members { get; set; } = new List<ChatMembership>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
