using System;
using Crew.Contracts.Chat;
using Crew.Domain.Entities;

namespace Crew.Api.Mapping;

public static class ChatMappings
{
    public static ChatMessageDto ToDto(this ChatMessage message)
        => new(
            message.Id,
            message.ChatId,
            message.SenderId,
            message.Kind,
            message.BodyText,
            message.MetaJson,
            message.CreatedAt,
            message.Seq,
            message.Status);

    public static ChatMemberDto ToDto(this ChatMember member)
        => new(
            member.ChatId,
            member.UserId,
            member.Role,
            member.JoinedAt,
            member.MutedUntil,
            member.LastReadMessageSeq,
            member.LeftAt);

    public static ChatSummaryDto ToSummaryDto(this Chat chat, ChatMember membership, ChatMessage? lastMessage, long lastSeq)
    {
        var unread = membership.LastReadMessageSeq.HasValue
            ? (int)Math.Max(0, lastSeq - membership.LastReadMessageSeq.Value)
            : (int)Math.Max(0, lastSeq);

        return new ChatSummaryDto(
            chat.Id,
            chat.Type,
            chat.Title,
            chat.OwnerUserId,
            chat.EventId,
            chat.IsArchived,
            chat.CreatedAt,
            membership.Role,
            membership.LastReadMessageSeq,
            lastSeq,
            unread,
            lastMessage?.ToDto());
    }
}
