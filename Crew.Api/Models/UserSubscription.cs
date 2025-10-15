using System;
using System.Runtime.Serialization;

namespace Crew.Api.Models;

public class UserSubscription
{
    public string UserUid { get; set; } = string.Empty;

    public int PlanId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public UserAccount? User { get; set; }

    public SubscriptionPlan? Plan { get; set; }
}

/// <summary>
/// Identifies the built-in subscription plan tiers.
/// </summary>
public enum SubscriptionPlanKey
{
    /// <summary>
    /// The complimentary plan granted to all users.
    /// </summary>
    [EnumMember(Value = "free")]
    Free,

    /// <summary>
    /// The first paid subscription tier.
    /// </summary>
    [EnumMember(Value = "tier1")]
    Tier1,

    /// <summary>
    /// The second paid subscription tier.
    /// </summary>
    [EnumMember(Value = "tier2")]
    Tier2,

    /// <summary>
    /// The highest paid subscription tier currently available.
    /// </summary>
    [EnumMember(Value = "tier3")]
    Tier3,
}
