using System;
using System.Collections.Generic;

namespace Crew.Application.Events;

public sealed record EventCard(
    Guid Id,
    Guid OwnerId,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset CreatedAt,
    double Latitude,
    double Longitude,
    double DistanceKm,
    int Registrations,
    int Likes,
    double Engagement,
    IReadOnlyList<string> Tags,
    DateTimeOffset LastModified);
