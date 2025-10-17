namespace Crew.Contracts.Events;

public sealed record EventSummaryDto(
    Guid Id,
    Guid OwnerId,
    string Title,
    DateTimeOffset StartTime,
    double[] Center,
    int MemberCount,
    int? MaxParticipants,
    bool IsRegistered,
    IReadOnlyList<string> Tags);
