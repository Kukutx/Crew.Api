using Crew.Application.Abstractions;
using Crew.Domain.Entities;

namespace Crew.Application.Auth;

public sealed class UserProvisioningService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserProvisioningService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<User> EnsureUserAsync(string firebaseUid, string? displayName, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.FindByFirebaseUidAsync(firebaseUid, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirebaseUid = firebaseUid,
            DisplayName = displayName,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user;
    }
}
