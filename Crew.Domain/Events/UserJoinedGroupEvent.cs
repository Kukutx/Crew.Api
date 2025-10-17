namespace Crew.Domain.Events;

public record UserJoinedGroupEvent(Guid ChatId, Guid UserId, DateTimeOffset JoinedAt);
