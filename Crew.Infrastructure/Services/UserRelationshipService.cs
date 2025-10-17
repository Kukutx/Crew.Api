using Crew.Application.Abstractions;
using Crew.Application.Users;
using Crew.Domain.Entities;

namespace Crew.Infrastructure.Services;

internal sealed class UserRelationshipService : IUserRelationshipService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserRelationshipService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task FollowAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default)
    {
        if (followerId == followingId)
        {
            throw new InvalidOperationException("Cannot follow yourself.");
        }

        var follower = await _userRepository.GetByIdAsync(followerId, cancellationToken)
            ?? throw new InvalidOperationException("Follower not found");
        var following = await _userRepository.GetByIdAsync(followingId, cancellationToken)
            ?? throw new InvalidOperationException("Target user not found");

        var existing = await _userRepository.GetFollowAsync(followerId, followingId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var follow = new UserFollow
        {
            FollowerId = follower.Id,
            FollowingId = following.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddFollowAsync(follow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnfollowAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetFollowAsync(followerId, followingId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        await _userRepository.RemoveFollowAsync(existing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserGuestbookItem> AddGuestbookEntryAsync(Guid ownerId, Guid authorId, string content, int? rating, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Content is required.");
        }

        var owner = await _userRepository.GetByIdAsync(ownerId, cancellationToken)
            ?? throw new InvalidOperationException("Owner not found");
        var author = await _userRepository.GetByIdAsync(authorId, cancellationToken)
            ?? throw new InvalidOperationException("Author not found");

        var entry = new UserGuestbookEntry
        {
            Id = Guid.NewGuid(),
            OwnerId = owner.Id,
            AuthorId = author.Id,
            Content = content.Trim(),
            Rating = rating,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddGuestbookEntryAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserGuestbookItem(
            entry.Id,
            entry.AuthorId,
            author.DisplayName,
            entry.Content,
            entry.Rating,
            entry.CreatedAt);
    }
}
