using Crew.Domain.Entities;

namespace Crew.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> FindByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<UserFollow?> GetFollowAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default);
    Task AddFollowAsync(UserFollow follow, CancellationToken cancellationToken = default);
    Task RemoveFollowAsync(UserFollow follow, CancellationToken cancellationToken = default);
    Task AddGuestbookEntryAsync(UserGuestbookEntry entry, CancellationToken cancellationToken = default);
}
