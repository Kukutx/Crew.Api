namespace Crew.Contracts.Moments;

public sealed record CreateMomentRequestDto(
    Guid? EventId,
    string Title,
    string? Content,
    string CoverImageUrl,
    string Country,
    string? City,
    IReadOnlyList<string> Images);
