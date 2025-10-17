using System;
using Crew.Domain.Enums;

namespace Crew.Contracts.Chat;

public sealed record ChatSummaryDto(
    Guid Id,
    ChatType Type,
    string? Title,
    Guid? OwnerUserId,
    Guid? EventId,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    ChatMemberRole Role,
    long? LastReadMessageSeq,
    long LastMessageSeq,
    int UnreadCount,
    ChatMessageDto? LastMessage);
