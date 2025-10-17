using Crew.Domain.Enums;

namespace Crew.Application.Users;

public sealed record UserEventParticipation(
    Guid EventId,
    string Title,
    DateTimeOffset StartTime,
    ActivityRole Role,
    bool IsCreator,
    int ConfirmedParticipants,
    int? MaxParticipants);
