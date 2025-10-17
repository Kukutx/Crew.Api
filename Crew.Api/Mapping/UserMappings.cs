using Crew.Application.Users;
using Crew.Contracts.Users;
using System.Linq;

namespace Crew.Api.Mapping;

public static class UserMappings
{
    public static UserProfileDto ToDto(this UserProfile profile)
    {
        return new UserProfileDto(
            profile.Id,
            profile.DisplayName,
            profile.Role.ToString().ToLowerInvariant(),
            profile.Bio,
            profile.AvatarUrl,
            profile.Followers,
            profile.Following,
            profile.Tags,
            profile.Activities.Select(ToDto).ToList(),
            profile.Guestbook.Select(ToDto).ToList(),
            profile.History.Select(ToDto).ToList());
    }

    public static UserActivityDto ToDto(this UserEventParticipation participation)
    {
        return new UserActivityDto(
            participation.EventId,
            participation.Title,
            participation.StartTime,
            participation.Role.ToString().ToLowerInvariant(),
            participation.IsCreator,
            participation.ConfirmedParticipants,
            participation.MaxParticipants);
    }

    public static UserGuestbookEntryDto ToDto(this UserGuestbookItem guestbook)
    {
        return new UserGuestbookEntryDto(
            guestbook.Id,
            guestbook.AuthorId,
            guestbook.AuthorDisplayName,
            guestbook.Content,
            guestbook.Rating,
            guestbook.CreatedAt);
    }

    public static UserHistoryDto ToDto(this UserHistoryItem history)
    {
        return new UserHistoryDto(
            history.Id,
            history.EventId,
            history.EventTitle,
            history.Role.ToString().ToLowerInvariant(),
            history.OccurredAt);
    }
}
