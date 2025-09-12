using System.Collections.Generic;

namespace Crew.Api.Models;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Location { get; set; } = "";
    public string Description { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public string CoverImageUrl { get; set; } = "";
}

