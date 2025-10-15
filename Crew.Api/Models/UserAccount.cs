using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crew.Api.Entities;

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
    public UserStatus Status { get; set; } = UserStatus.Active;

    [MaxLength(32)]
    public string IdentityLabel { get; set; } = UserIdentityLabels.Visitor;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
        = null;

    public ICollection<UserRoleAssignment> Roles { get; set; } = new List<UserRoleAssignment>();

    public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Event> Events { get; set; } = new List<Event>();

    public ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();

    public ICollection<UserFollow> Following { get; set; } = new List<UserFollow>();
    public ICollection<EventFavorite> FavoriteEvents { get; set; } = new List<EventFavorite>();
    public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();

    public ICollection<TripParticipant> TripParticipations { get; set; } = new List<TripParticipant>();
    public ICollection<TripFavorite> TripFavorites { get; set; } = new List<TripFavorite>();
    public ICollection<TripComment> TripComments { get; set; } = new List<TripComment>();
    public ICollection<TripImage> TripImages { get; set; } = new List<TripImage>();
}

public static class UserIdentityLabels
{
    public const string Visitor = "游客";
    public const string Participant = "参与者";
    public const string Organizer = "组织者";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Visitor,
        Participant,
        Organizer,
    };
}
