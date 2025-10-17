namespace Crew.Application.Users;

public sealed record UserGuestbookItem(
    Guid Id,
    Guid AuthorId,
    string? AuthorDisplayName,
    string Content,
    int? Rating,
    DateTimeOffset CreatedAt);
