namespace Crew.Application.Moments;

public sealed record MomentCommentModel(
    Guid Id,
    Guid AuthorId,
    string? AuthorDisplayName,
    string Content,
    DateTimeOffset CreatedAt);
