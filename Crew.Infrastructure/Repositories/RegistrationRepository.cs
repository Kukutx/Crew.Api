using Crew.Application.Abstractions;
using Crew.Domain.Entities;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crew.Infrastructure.Repositories;

internal sealed class RegistrationRepository : IRegistrationRepository
{
    private readonly AppDbContext _dbContext;

    public RegistrationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Registration?> GetAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
        => _dbContext.Registrations.FirstOrDefaultAsync(x => x.EventId == eventId && x.UserId == userId, cancellationToken);

    public Task<int> CountConfirmedAsync(Guid eventId, CancellationToken cancellationToken = default)
        => _dbContext.Registrations.CountAsync(x => x.EventId == eventId && x.Status == Crew.Domain.Enums.RegistrationStatus.Confirmed, cancellationToken);

    public Task AddAsync(Registration registration, CancellationToken cancellationToken = default)
        => _dbContext.Registrations.AddAsync(registration, cancellationToken).AsTask();

    public Task RemoveAsync(Registration registration)
    {
        _dbContext.Registrations.Remove(registration);
        return Task.CompletedTask;
    }
}
