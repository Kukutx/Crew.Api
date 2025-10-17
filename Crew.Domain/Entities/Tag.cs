using Crew.Domain.Enums;

namespace Crew.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public TagCategory Category { get; set; } = TagCategory.General;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<UserTag> UserTags { get; set; } = new List<UserTag>();
    public ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();
}
