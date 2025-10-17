namespace Crew.Contracts.Moments;

public sealed record MomentSummaryDto(
    Guid Id,
    Guid UserId,
    string? UserDisplayName,
    string Title,
    string CoverImageUrl,
    string Country,
    string? City,
    DateTimeOffset CreatedAt);
