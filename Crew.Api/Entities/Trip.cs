using System;
using System.Collections.Generic;
namespace Crew.Api.Entities;

public class Trip : Event
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string StartLocation { get; set; } = string.Empty;
    public string EndLocation { get; set; } = string.Empty;

    public string? ItineraryDescription { get; set; }

    public ICollection<TripRoute> Routes { get; set; } = new List<TripRoute>();
    public ICollection<TripSchedule> Schedules { get; set; } = new List<TripSchedule>();
    public ICollection<TripParticipant> Participants { get; set; } = new List<TripParticipant>();
    public ICollection<TripComment> TripComments { get; set; } = new List<TripComment>();
    public ICollection<TripFavorite> TripFavorites { get; set; } = new List<TripFavorite>();
    public ICollection<TripImage> TripImages { get; set; } = new List<TripImage>();
}
