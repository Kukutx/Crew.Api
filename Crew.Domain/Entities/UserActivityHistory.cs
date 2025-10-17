using Crew.Domain.Enums;

namespace Crew.Domain.Entities;

public class UserActivityHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public ActivityRole Role { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    public User User { get; set; } = null!;
    public RoadTripEvent Event { get; set; } = null!;
}
