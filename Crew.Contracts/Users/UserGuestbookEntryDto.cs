namespace Crew.Contracts.Users;

public sealed record UserGuestbookEntryDto(
    Guid Id,
    Guid AuthorId,
    string? AuthorDisplayName,
    string Content,
    int? Rating,
    DateTimeOffset CreatedAt);
