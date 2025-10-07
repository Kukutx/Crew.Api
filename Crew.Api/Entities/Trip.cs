using System;
using System.Collections.Generic;

namespace Crew.Api.Entities;

public class Trip : Event
{
    public string StartLocation { get; set; } = string.Empty;
    public string EndLocation { get; set; } = string.Empty;
    public int ExpectedParticipants { get; set; }
    public double? StartLatitude { get; set; }
    public double? StartLongitude { get; set; }
    public double? EndLatitude { get; set; }
    public double? EndLongitude { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;

    public ICollection<TripRoute> Routes { get; set; } = new List<TripRoute>();
    public ICollection<TripSchedule> Schedules { get; set; } = new List<TripSchedule>();
    public ICollection<TripParticipant> Participants { get; set; } = new List<TripParticipant>();
    public ICollection<TripComment> Comments { get; set; } = new List<TripComment>();
    public ICollection<TripFavorite> Favorites { get; set; } = new List<TripFavorite>();
    public ICollection<TripImage> Images { get; set; } = new List<TripImage>();
}

public static class TripStatuses
{
    public const string Draft = EventStatuses.Draft;
    public const string Planning = EventStatuses.Planning;
    public const string Published = EventStatuses.Published;
    public const string Completed = EventStatuses.Completed;
    public const string Cancelled = EventStatuses.Cancelled;

    public static IReadOnlySet<string> All => EventStatuses.All;

    public static bool IsValid(string? value)
        => EventStatuses.IsValid(value);
}
