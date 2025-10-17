namespace Crew.Contracts.Users;

public sealed record EnsureUserRequest(
    string FirebaseUid,
    string? DisplayName,
    string Role,
    string? AvatarUrl,
    string? Bio,
    IReadOnlyList<string>? Tags);
