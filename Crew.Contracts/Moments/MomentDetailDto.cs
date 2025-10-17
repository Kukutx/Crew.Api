namespace Crew.Contracts.Moments;

public sealed record MomentDetailDto(
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
    IReadOnlyList<MomentCommentDto> Comments);
