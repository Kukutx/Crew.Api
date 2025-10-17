namespace Crew.Application.Users;

public interface IUserRelationshipService
{
    Task FollowAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default);
    Task UnfollowAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default);
    Task<UserGuestbookItem> AddGuestbookEntryAsync(Guid ownerId, Guid authorId, string content, int? rating, CancellationToken cancellationToken = default);
}
