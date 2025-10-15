using Crew.Domain.Entities;

namespace Crew.Application.Abstractions;

public interface IRegistrationRepository
{
    Task<Registration?> GetAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountConfirmedAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task AddAsync(Registration registration, CancellationToken cancellationToken = default);
    Task RemoveAsync(Registration registration);
}
