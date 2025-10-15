namespace Crew.Application.Events;

public sealed record EventSegmentModel(int Seq, double Longitude, double Latitude, string? Note);
