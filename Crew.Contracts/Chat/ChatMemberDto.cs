using System;
using Crew.Domain.Enums;

namespace Crew.Contracts.Chat;

public sealed record ChatMemberDto(
    Guid ChatId,
    Guid UserId,
    ChatMemberRole Role,
    DateTimeOffset JoinedAt,
    DateTimeOffset? MutedUntil,
    long? LastReadMessageSeq,
    DateTimeOffset? LeftAt);
