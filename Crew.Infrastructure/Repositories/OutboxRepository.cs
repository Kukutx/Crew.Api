using Crew.Application.Abstractions;
using Crew.Domain.Entities;
using Crew.Infrastructure.Persistence;

namespace Crew.Infrastructure.Repositories;

internal sealed class OutboxRepository : IOutboxRepository
{
    private readonly AppDbContext _dbContext;

    public OutboxRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        => _dbContext.OutboxMessages.AddAsync(message, cancellationToken).AsTask();
}
