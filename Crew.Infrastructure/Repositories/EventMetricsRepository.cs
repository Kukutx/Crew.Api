using System;
using System.Threading;
using System.Threading.Tasks;
using Crew.Application.Abstractions;
using Crew.Domain.Entities;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crew.Infrastructure.Repositories;

internal sealed class EventMetricsRepository : IEventMetricsRepository
{
    private readonly AppDbContext _dbContext;

    public EventMetricsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EventMetrics> GetOrCreateAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var metrics = await _dbContext.EventMetrics.FirstOrDefaultAsync(x => x.EventId == eventId, cancellationToken);
        if (metrics is not null)
        {
            return metrics;
        }

        metrics = new EventMetrics
        {
            EventId = eventId,
            LikesCount = 0,
            RegistrationsCount = 0,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.EventMetrics.AddAsync(metrics, cancellationToken);
        return metrics;
    }
}
