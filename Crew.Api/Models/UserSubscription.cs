using System;

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

public static class SubscriptionPlanKeys
{
    public const string Free = "free";
    public const string Tier1 = "tier1";
    public const string Tier2 = "tier2";
    public const string Tier3 = "tier3";
}
