using System;
using Crew.Domain.Enums;

namespace Crew.Contracts.Chat;

public sealed record ChatMessageDto(
    long Id,
    Guid ChatId,
    Guid SenderId,
    ChatMessageKind Kind,
    string? BodyText,
    string? MetaJson,
    DateTimeOffset CreatedAt,
    long Seq,
    ChatMessageStatus Status);
