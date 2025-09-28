using System.Text.Json.Serialization;

namespace Crew.Api.Models;

public class SubscriptionPlan
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public ICollection<DomainUsers> Users { get; set; } = new List<DomainUsers>();
}
