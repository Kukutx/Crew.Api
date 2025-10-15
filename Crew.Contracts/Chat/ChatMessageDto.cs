namespace Crew.Contracts.Chat;

public sealed record ChatMessageDto(Guid Id, Guid GroupId, Guid SenderId, string Content, DateTimeOffset SentAt);
