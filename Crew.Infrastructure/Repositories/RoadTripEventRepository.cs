using Crew.Application.Abstractions;
using Crew.Domain.Entities;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crew.Infrastructure.Repositories;

internal sealed class RoadTripEventRepository : IRoadTripEventRepository
{
    private readonly AppDbContext _dbContext;

    public RoadTripEventRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RoadTripEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.RoadTripEvents.Include(x => x.Registrations).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(RoadTripEvent entity, CancellationToken cancellationToken = default)
        => _dbContext.RoadTripEvents.AddAsync(entity, cancellationToken).AsTask();
}
