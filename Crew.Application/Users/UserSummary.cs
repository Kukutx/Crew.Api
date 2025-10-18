using Crew.Domain.Enums;

namespace Crew.Application.Users;

public sealed record UserSummary(
    Guid Id,
    string? DisplayName,
    string? Email,
    UserRole Role,
    string? AvatarUrl,
    DateTimeOffset CreatedAt);
