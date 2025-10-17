using System;

namespace Crew.Contracts.Chat;

public sealed record ChatReactionDto(
    long MessageId,
    Guid UserId,
    string Emoji,
    DateTimeOffset CreatedAt);
