using System;
namespace Crew.Api.Models;

public class UserFollow
{
    public string FollowerUid { get; set; } = string.Empty;

    public string FollowedUid { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UserAccount? Follower { get; set; }

    public UserAccount? Followed { get; set; }
}
