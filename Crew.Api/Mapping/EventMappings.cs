using Crew.Application.Events;
using Crew.Application.Moments;
using Crew.Contracts.Events;
using System;

namespace Crew.Api.Mapping;

public static class EventMappings
{
    public static EventSummaryDto ToDto(this EventSummary summary)
        => new(
            summary.Id,
            summary.OwnerId,
            summary.Title,
            summary.StartTime,
            new[] { summary.Longitude, summary.Latitude },
            summary.MemberCount,
            summary.MaxParticipants,
            summary.IsRegistered,
            summary.Tags);

    public static EventDetailDto ToDto(this EventDetail detail)
        => new(
            detail.Id,
            detail.OwnerId,
            detail.Title,
            detail.Description,
            detail.StartTime,
            detail.EndTime,
            new[] { detail.StartLongitude, detail.StartLatitude },
            detail.EndLongitude is null || detail.EndLatitude is null ? null : new[] { detail.EndLongitude.Value, detail.EndLatitude.Value },
            detail.RoutePolyline,
            detail.MaxParticipants,
            detail.Visibility,
            detail.Segments.Select(s => new EventSegmentDto(s.Seq, new[] { s.Longitude, s.Latitude }, s.Note)).ToList(),
            detail.MemberCount,
            detail.IsRegistered,
            detail.Tags,
            (detail.Moments ?? Array.Empty<MomentSummary>()).Select(m => m.ToDto()).ToList());

    public static EventCardDto ToDto(this EventCard card)
        => new(
            card.Id,
            card.OwnerId,
            card.Title,
            card.Description,
            card.StartTime,
            card.CreatedAt,
            new[] { card.Longitude, card.Latitude },
            card.DistanceKm,
            card.Registrations,
            card.Likes,
            card.Engagement,
            card.Tags);
}
