namespace Crew.Application.Moments;

public sealed record CreateMomentRequest(
    Guid UserId,
    Guid? EventId,
    string Title,
    string? Content,
    string CoverImageUrl,
    string Country,
    string? City,
    IReadOnlyList<string> Images);
