using System.Collections.Generic;

namespace Crew.Application.Events;

public sealed record GetFeedQuery(
    double Latitude,
    double Longitude,
    double RadiusKm,
    int Limit,
    string? Cursor,
    IReadOnlyList<string>? Tags);
