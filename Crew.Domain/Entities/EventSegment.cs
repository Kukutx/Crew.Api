using NetTopologySuite.Geometries;

namespace Crew.Domain.Entities;

public class EventSegment
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public int Seq { get; set; }
    public Point Waypoint { get; set; } = null!;
    public string? Note { get; set; }
    public RoadTripEvent? Event { get; set; }
}
