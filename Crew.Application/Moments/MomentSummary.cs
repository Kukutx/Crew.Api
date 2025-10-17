namespace Crew.Application.Moments;

public sealed record MomentSummary(
    Guid Id,
    Guid UserId,
    string? UserDisplayName,
    string Title,
    string CoverImageUrl,
    string Country,
    string? City,
    DateTimeOffset CreatedAt);
