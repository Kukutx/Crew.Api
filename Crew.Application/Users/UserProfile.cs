using Crew.Domain.Enums;

namespace Crew.Application.Users;

public sealed record UserProfile(
    Guid Id,
    string? DisplayName,
    string? Email,
    UserRole Role,
    string? Bio,
    string? AvatarUrl,
    int Followers,
    int Following,
    IReadOnlyList<string> Tags,
    IReadOnlyList<UserEventParticipation> Activities,
    IReadOnlyList<UserGuestbookItem> Guestbook,
    IReadOnlyList<UserHistoryItem> History);
