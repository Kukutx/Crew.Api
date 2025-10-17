using Crew.Domain.Entities;

namespace Crew.Application.Abstractions;

public interface IEventMetricsRepository
{
    Task<EventMetrics> GetOrCreateAsync(Guid eventId, CancellationToken cancellationToken = default);
}
