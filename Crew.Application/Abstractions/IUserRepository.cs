using Crew.Domain.Entities;

namespace Crew.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> FindByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
