using System;
using System.Collections.Generic;

namespace Crew.Api.Models;

/// <summary>
/// Simplified event model exposed to the client.
/// </summary>
public class EventModal
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Organizer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ExpectedParticipants { get; set; }
    public string UserUid { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public List<string> ImageUrls { get; set; } = new();
    public string CoverImageUrl { get; set; } = string.Empty;
}
