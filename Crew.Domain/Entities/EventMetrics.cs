using System;

namespace Crew.Domain.Entities;

public class EventMetrics
{
    public Guid EventId { get; set; }
    public RoadTripEvent Event { get; set; } = null!;
    public int LikesCount { get; set; }
    public int RegistrationsCount { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
