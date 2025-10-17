namespace Crew.Application.Moments;

public sealed record MomentDetail(
    Guid Id,
    Guid UserId,
    string? UserDisplayName,
    string Title,
    string? Content,
    string CoverImageUrl,
    string Country,
    string? City,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> Images,
    IReadOnlyList<MomentCommentModel> Comments);
