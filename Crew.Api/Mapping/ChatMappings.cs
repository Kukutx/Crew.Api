using Crew.Contracts.Chat;
using Crew.Domain.Entities;

namespace Crew.Api.Mapping;

public static class ChatMappings
{
    public static ChatMessageDto ToDto(this ChatMessage message)
        => new(message.Id, message.GroupId, message.SenderId, message.Content, message.SentAt);

    public static PrivateMessageDto ToDto(this PrivateMessage message)
        => new(message.Id, message.DialogId, message.SenderId, message.Content, message.SentAt);
}
