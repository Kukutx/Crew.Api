using System;
using System.Collections.Generic;

namespace Crew.Api.Models;

public class EventInputModel
{
    public string? Title { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public int Participants { get; set; }

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public List<string>? ImageUrls { get; set; } = new();
    public string? CoverImageUrl { get; set; }
}
