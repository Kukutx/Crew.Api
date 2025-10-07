using System;
using System.Collections.Generic;

namespace Crew.Api.Models;

public class TripInputModel
{
    public string? Title { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? StartLocation { get; set; }
    public string? EndLocation { get; set; }
    public int? ExpectedParticipants { get; set; }
    public double? StartLatitude { get; set; }
    public double? StartLongitude { get; set; }
    public double? EndLatitude { get; set; }
    public double? EndLongitude { get; set; }
    public List<TripRouteInput>? Routes { get; set; }
    public List<TripScheduleInput>? Schedules { get; set; }
    public string? CoverImageUrl { get; set; }
}

public record TripRouteInput(int OrderIndex, string Name, double? Latitude, double? Longitude, string? Description);

public record TripScheduleInput(DateTime Date, string? Content, string? Hotel, string? Meal, string? Note);
