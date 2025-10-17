namespace Crew.Application.Events;

public sealed record EventSummary(
    Guid Id,
    Guid OwnerId,
    string Title,
    DateTimeOffset StartTime,
    double Longitude,
    double Latitude,
    int MemberCount,
    int? MaxParticipants,
    bool IsRegistered,
    IReadOnlyList<string> Tags);
