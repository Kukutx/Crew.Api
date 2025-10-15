namespace Crew.Domain.Events;

public record UserJoinedGroupEvent(Guid GroupId, Guid UserId, DateTimeOffset JoinedAt);
