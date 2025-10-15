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
    {
        return _dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }
}
