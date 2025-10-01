using System.Text.Json.Serialization;

namespace Crew.Api.Models;

public class SubscriptionPlan
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    [JsonIgnore]
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
