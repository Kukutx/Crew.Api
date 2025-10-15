namespace Crew.Application.Events;

public sealed record EventSummary(
    Guid Id,
    string Title,
    DateTimeOffset StartTime,
    double Longitude,
    double Latitude,
    int MemberCount,
    bool IsRegistered);
