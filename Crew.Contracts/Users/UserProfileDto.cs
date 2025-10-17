namespace Crew.Contracts.Users;

public sealed record UserProfileDto(
    Guid Id,
    string? DisplayName,
    string Role,
    string? Bio,
    string? AvatarUrl,
    int Followers,
    int Following,
    IReadOnlyList<string> Tags,
    IReadOnlyList<UserActivityDto> Activities,
    IReadOnlyList<UserGuestbookEntryDto> Guestbook,
    IReadOnlyList<UserHistoryDto> History);
