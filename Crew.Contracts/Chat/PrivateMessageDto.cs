namespace Crew.Contracts.Chat;

public sealed record PrivateMessageDto(Guid Id, Guid DialogId, Guid SenderId, string Content, DateTimeOffset SentAt);
