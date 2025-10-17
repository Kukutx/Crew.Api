namespace Crew.Domain.Entities;

public class UserFollow
{
    public Guid FollowerId { get; set; }
    public Guid FollowingId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User Follower { get; set; } = null!;
    public User Following { get; set; } = null!;
}
