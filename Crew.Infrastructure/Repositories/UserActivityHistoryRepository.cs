using Crew.Application.Abstractions;
using Crew.Domain.Entities;
using Crew.Domain.Enums;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crew.Infrastructure.Repositories;

internal sealed class UserActivityHistoryRepository : IUserActivityHistoryRepository
{
    private readonly AppDbContext _dbContext;

    public UserActivityHistoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserActivityHistory?> FindAsync(Guid userId, Guid eventId, ActivityRole role, CancellationToken cancellationToken = default)
    {
        return _dbContext.UserActivityHistories
            .FirstOrDefaultAsync(x => x.UserId == userId && x.EventId == eventId && x.Role == role, cancellationToken);
    }

    public Task AddAsync(UserActivityHistory history, CancellationToken cancellationToken = default)
    {
        return _dbContext.UserActivityHistories.AddAsync(history, cancellationToken).AsTask();
    }

    public Task RemoveAsync(UserActivityHistory history, CancellationToken cancellationToken = default)
    {
        _dbContext.UserActivityHistories.Remove(history);
        return Task.CompletedTask;
    }
}
