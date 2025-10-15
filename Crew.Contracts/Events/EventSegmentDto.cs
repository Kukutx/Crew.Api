namespace Crew.Contracts.Events;

public sealed record EventSegmentDto(int Seq, double[] Waypoint, string? Note);
