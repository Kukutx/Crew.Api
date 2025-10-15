using System;
using System.Collections.Generic;
using Crew.Api.Models;

namespace Crew.Api.Entities;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public string UserUid { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public List<string> ImageUrls { get; set; } = new();
    public string CoverImageUrl { get; set; } = string.Empty;

    public UserAccount? User { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<EventFavorite> Favorites { get; set; } = new List<EventFavorite>();
    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
}
