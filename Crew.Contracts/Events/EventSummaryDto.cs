namespace Crew.Contracts.Events;

public sealed record EventSummaryDto(
    Guid Id,
    string Title,
    DateTimeOffset StartTime,
    double[] Center,
    int MemberCount,
    bool IsRegistered);
