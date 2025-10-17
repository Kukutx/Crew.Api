namespace Crew.Domain.Entities;

public class EventTag
{
    public Guid EventId { get; set; }
    public Guid TagId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public RoadTripEvent Event { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
