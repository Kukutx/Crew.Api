using Crew.Application.Abstractions;
using Crew.Domain.Entities;
using Crew.Domain.Enums;

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

    public async Task<User> EnsureUserAsync(
        string firebaseUid,
        string? displayName,
        string? email = null,
        UserRole role = UserRole.User,
        string? avatarUrl = null,
        CancellationToken cancellationToken = default)
    {
        var emailProvided = email is not null;
        var normalizedEmail = string.IsNullOrWhiteSpace(email) ? null : email?.Trim();

        var existing = await _userRepository.FindByFirebaseUidAsync(firebaseUid, cancellationToken);
        if (existing is not null)
        {
            var requiresUpdate = false;

            if (!string.IsNullOrWhiteSpace(displayName) && !string.Equals(existing.DisplayName, displayName, StringComparison.Ordinal))
            {
                existing.DisplayName = displayName;
                requiresUpdate = true;
            }

            if (emailProvided && !string.Equals(existing.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                existing.Email = normalizedEmail;
                requiresUpdate = true;
            }

            if (!string.IsNullOrWhiteSpace(avatarUrl) && !string.Equals(existing.AvatarUrl, avatarUrl, StringComparison.Ordinal))
            {
                existing.AvatarUrl = avatarUrl;
                requiresUpdate = true;
            }

            if (existing.Role != role)
            {
                existing.Role = role;
                requiresUpdate = true;
            }

            if (requiresUpdate)
            {
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return existing;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirebaseUid = firebaseUid,
            DisplayName = displayName,
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            Role = role,
            AvatarUrl = avatarUrl,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user;
    }
}
