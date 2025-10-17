using Crew.Contracts.Moments;
using Crew.Domain.Enums;

namespace Crew.Contracts.Events;

public sealed record EventDetailDto(
    Guid Id,
    Guid OwnerId,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    double[] StartPoint,
    double[]? EndPoint,
    string? RoutePolyline,
    int? MaxParticipants,
    EventVisibility Visibility,
    IReadOnlyList<EventSegmentDto> Segments,
    int MemberCount,
    bool IsRegistered,
    IReadOnlyList<string> Tags,
    IReadOnlyList<MomentSummaryDto> Moments);
