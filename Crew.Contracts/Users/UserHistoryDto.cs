namespace Crew.Contracts.Users;

public sealed record UserHistoryDto(
    Guid Id,
    Guid EventId,
    string EventTitle,
    string Role,
    DateTimeOffset OccurredAt);
