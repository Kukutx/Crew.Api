using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crew.Api.Models;

public class UserFollow
{
    [Key, Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string FollowerUid { get; set; } = string.Empty;

    public UserAccount? Follower { get; set; }
        = null;

    [Key, Column(Order = 1)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string FollowedUid { get; set; } = string.Empty;

    public UserAccount? Followed { get; set; }
        = null;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
