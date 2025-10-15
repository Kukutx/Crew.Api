using Crew.Domain.Enums;
using NetTopologySuite.Geometries;

namespace Crew.Domain.Entities;

public class RoadTripEvent
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public Point StartPoint { get; set; } = null!;
    public Point? EndPoint { get; set; }
    public string? RoutePolyline { get; set; }
    public int? MaxParticipants { get; set; }
    public EventVisibility Visibility { get; set; }
    public ICollection<EventSegment> Segments { get; set; } = new List<EventSegment>();
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
