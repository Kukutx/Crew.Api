using Crew.Application.Users;
using Crew.Domain.Enums;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crew.Infrastructure.Services;

internal sealed class UserReadService : IUserReadService
{
    private readonly AppDbContext _dbContext;

    public UserReadService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserProfile?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Tags)
                .ThenInclude(ut => ut.Tag)
            .Include(u => u.GuestbookEntries)
                .ThenInclude(g => g.Author)
            .Include(u => u.ActivityHistory)
                .ThenInclude(h => h.Event)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var followerCount = await _dbContext.UserFollows.CountAsync(x => x.FollowingId == userId, cancellationToken);
        var followingCount = await _dbContext.UserFollows.CountAsync(x => x.FollowerId == userId, cancellationToken);

        var activityByEvent = new Dictionary<Guid, UserEventParticipation>();

        var createdEvents = await _dbContext.RoadTripEvents
            .Include(e => e.Registrations)
            .Where(e => e.OwnerId == userId)
            .ToListAsync(cancellationToken);

        foreach (var created in createdEvents)
        {
            var confirmedParticipants = created.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);
            activityByEvent[created.Id] = new UserEventParticipation(
                created.Id,
                created.Title,
                created.StartTime,
                ActivityRole.Creator,
                true,
                confirmedParticipants,
                created.MaxParticipants);
        }

        var joinedRegistrations = await _dbContext.Registrations
            .Include(r => r.Event)!
                .ThenInclude(e => e!.Registrations)
            .Where(r => r.UserId == userId && r.Status == RegistrationStatus.Confirmed)
            .ToListAsync(cancellationToken);

        foreach (var registration in joinedRegistrations)
        {
            if (registration.Event is null)
            {
                continue;
            }

            if (!activityByEvent.TryGetValue(registration.EventId, out var existing))
            {
                var confirmedParticipants = registration.Event.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);
                var isCreator = registration.Event.OwnerId == userId;
                var role = isCreator ? ActivityRole.Creator : ActivityRole.Participant;

                activityByEvent[registration.EventId] = new UserEventParticipation(
                    registration.EventId,
                    registration.Event.Title,
                    registration.Event.StartTime,
                    role,
                    isCreator,
                    confirmedParticipants,
                    registration.Event.MaxParticipants);
            }
            else if (!existing.IsCreator)
            {
                activityByEvent[registration.EventId] = existing with
                {
                    ConfirmedParticipants = registration.Event.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed)
                };
            }
        }

        var activities = activityByEvent.Values
            .OrderByDescending(a => a.StartTime)
            .ToList();

        var guestbook = user.GuestbookEntries
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new UserGuestbookItem(
                g.Id,
                g.AuthorId,
                g.Author?.DisplayName,
                g.Content,
                g.Rating,
                g.CreatedAt))
            .ToList();

        var history = user.ActivityHistory
            .Where(h => h.Event is not null)
            .OrderByDescending(h => h.OccurredAt)
            .Select(h => new UserHistoryItem(
                h.Id,
                h.EventId,
                h.Event!.Title,
                h.Role,
                h.OccurredAt))
            .ToList();

        var tags = user.Tags
            .Where(t => t.Tag is not null)
            .Select(t => t.Tag!.Name)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        return new UserProfile(
            user.Id,
            user.DisplayName,
            user.Role,
            user.Bio,
            user.AvatarUrl,
            followerCount,
            followingCount,
            tags,
            activities,
            guestbook,
            history);
    }
}
