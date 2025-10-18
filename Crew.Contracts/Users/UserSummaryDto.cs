namespace Crew.Contracts.Users;

public sealed record UserSummaryDto(
    Guid Id,
    string? DisplayName,
    string? Email,
    string Role,
    string? AvatarUrl,
    DateTimeOffset CreatedAt);
