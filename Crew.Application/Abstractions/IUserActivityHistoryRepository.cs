using Crew.Domain.Entities;
using Crew.Domain.Enums;

namespace Crew.Application.Abstractions;

public interface IUserActivityHistoryRepository
{
    Task<UserActivityHistory?> FindAsync(Guid userId, Guid eventId, ActivityRole role, CancellationToken cancellationToken = default);
    Task AddAsync(UserActivityHistory history, CancellationToken cancellationToken = default);
    Task RemoveAsync(UserActivityHistory history, CancellationToken cancellationToken = default);
}
