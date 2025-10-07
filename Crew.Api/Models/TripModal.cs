using System;
using System.Collections.Generic;
using Crew.Api.Entities;

namespace Crew.Api.Models;

public class TripModal
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = TripStatuses.Draft;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string StartLocation { get; set; } = string.Empty;
    public string EndLocation { get; set; } = string.Empty;
    public int ExpectedParticipants { get; set; }
    public double? StartLatitude { get; set; }
    public double? StartLongitude { get; set; }
    public double? EndLatitude { get; set; }
    public double? EndLongitude { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public string OrganizerUid { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    public IReadOnlyList<TripRoute> Routes { get; set; } = Array.Empty<TripRoute>();
    public IReadOnlyList<TripSchedule> Schedules { get; set; } = Array.Empty<TripSchedule>();
}
