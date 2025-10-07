using System;

namespace Crew.Api.Entities;

public class TripRoute
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int OrderIndex { get; set; }
    public string Name { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Description { get; set; } = string.Empty;

    public Trip? Trip { get; set; }
}
