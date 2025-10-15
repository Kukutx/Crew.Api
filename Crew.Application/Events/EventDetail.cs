using Crew.Domain.Enums;

namespace Crew.Application.Events;

public sealed record EventDetail(
    Guid Id,
    Guid OwnerId,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    double StartLongitude,
    double StartLatitude,
    double? EndLongitude,
    double? EndLatitude,
    string? RoutePolyline,
    int? MaxParticipants,
    EventVisibility Visibility,
    IReadOnlyList<EventSegmentModel> Segments,
    int MemberCount,
    bool IsRegistered);
