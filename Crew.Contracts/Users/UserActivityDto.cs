namespace Crew.Contracts.Users;

public sealed record UserActivityDto(
    Guid EventId,
    string Title,
    DateTimeOffset StartTime,
    string Role,
    bool IsCreator,
    int ConfirmedParticipants,
    int? MaxParticipants);
