using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crew.Api.Models;

public class UserAccount
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Uid { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(128)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(512)]
    public string Bio { get; set; } = string.Empty;

    [MaxLength(512)]
    public string AvatarUrl { get; set; } = AvatarDefaults.FallbackUrl;

    [MaxLength(512)]
    public string CoverImageUrl { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Status { get; set; } = UserStatuses.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
        = null;

    public ICollection<UserRoleAssignment> Roles { get; set; } = new List<UserRoleAssignment>();

    public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();

    public ICollection<UserFollow> Following { get; set; } = new List<UserFollow>();
}

public static class UserStatuses
{
    public const string Active = "active";
    public const string Suspended = "suspended";
}
