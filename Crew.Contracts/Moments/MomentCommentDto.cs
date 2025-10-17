namespace Crew.Contracts.Moments;

public sealed record MomentCommentDto(
    Guid Id,
    Guid AuthorId,
    string? AuthorDisplayName,
    string Content,
    DateTimeOffset CreatedAt);
