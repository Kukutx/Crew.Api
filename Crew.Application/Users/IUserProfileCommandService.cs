namespace Crew.Application.Users;

public interface IUserProfileCommandService
{
    Task UpdateProfileAsync(Guid userId, string? bio, string? avatarUrl, IEnumerable<string>? tags, CancellationToken cancellationToken = default);
}
