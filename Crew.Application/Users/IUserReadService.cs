namespace Crew.Application.Users;

public interface IUserReadService
{
    Task<UserProfile?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken cancellationToken = default);
}
