namespace Crew.Domain.Entities;

public class UserTag
{
    public Guid UserId { get; set; }
    public Guid TagId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
