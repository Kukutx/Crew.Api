using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
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
    public UserIdentityLabel IdentityLabel { get; set; } = UserIdentityLabel.Visitor;

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

/// <summary>
/// Represents the moderation status of a user account.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// The account is active and can access all permitted features.
    /// </summary>
    [EnumMember(Value = "active")]
    Active,

    /// <summary>
    /// The account is suspended and restricted from using the service.
    /// </summary>
    [EnumMember(Value = "suspended")]
    Suspended,
}

/// <summary>
/// Identifies a user's relationship to the platform's travel experiences.
/// </summary>
public enum UserIdentityLabel
{
    /// <summary>
    /// The user is browsing as a visitor.
    /// </summary>
    [EnumMember(Value = "游客")]
    Visitor,

    /// <summary>
    /// The user regularly participates in events or trips.
    /// </summary>
    [EnumMember(Value = "参与者")]
    Participant,

    /// <summary>
    /// The user organizes events or trips for others.
    /// </summary>
    [EnumMember(Value = "组织者")]
    Organizer,
}
