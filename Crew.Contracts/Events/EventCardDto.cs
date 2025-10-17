using System;
using System.Collections.Generic;

namespace Crew.Contracts.Events;

public sealed record EventCardDto(
    Guid Id,
    Guid OwnerId,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset CreatedAt,
    double[] Coordinates,
    double DistanceKm,
    int Registrations,
    int Likes,
    double Engagement,
    IReadOnlyList<string> Tags);
