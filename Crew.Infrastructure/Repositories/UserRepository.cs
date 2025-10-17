using Crew.Application.Abstractions;
using Crew.Domain.Entities;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crew.Infrastructure.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> FindByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken = default)
        => _dbContext.Users.FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
        => _dbContext.Users.AddAsync(user, cancellationToken).AsTask();

    public Task<UserFollow?> GetFollowAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default)
        => _dbContext.UserFollows.FirstOrDefaultAsync(x => x.FollowerId == followerId && x.FollowingId == followingId, cancellationToken);

    public Task AddFollowAsync(UserFollow follow, CancellationToken cancellationToken = default)
        => _dbContext.UserFollows.AddAsync(follow, cancellationToken).AsTask();

    public Task RemoveFollowAsync(UserFollow follow, CancellationToken cancellationToken = default)
    {
        _dbContext.UserFollows.Remove(follow);
        return Task.CompletedTask;
    }

    public Task AddGuestbookEntryAsync(UserGuestbookEntry entry, CancellationToken cancellationToken = default)
        => _dbContext.UserGuestbookEntries.AddAsync(entry, cancellationToken).AsTask();
}
