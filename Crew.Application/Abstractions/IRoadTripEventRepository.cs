using Crew.Domain.Entities;

namespace Crew.Application.Abstractions;

public interface IRoadTripEventRepository
{
    Task<RoadTripEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(RoadTripEvent entity, CancellationToken cancellationToken = default);
}
