using Crew.Domain.Enums;

namespace Crew.Application.Users;

public sealed record UserHistoryItem(
    Guid Id,
    Guid EventId,
    string EventTitle,
    ActivityRole Role,
    DateTimeOffset OccurredAt);
