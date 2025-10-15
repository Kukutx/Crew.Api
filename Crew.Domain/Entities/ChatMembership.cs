namespace Crew.Domain.Entities;

public class ChatMembership
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = null!;
    public DateTimeOffset JoinedAt { get; set; }
    public ChatGroup? Group { get; set; }
    public User? User { get; set; }
}
