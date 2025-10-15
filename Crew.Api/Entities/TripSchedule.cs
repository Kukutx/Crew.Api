using System;

namespace Crew.Api.Entities;

public class TripSchedule
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public DateTime Date { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Hotel { get; set; }
    public string? Meal { get; set; }
    public string? Note { get; set; }

    public Trip? Trip { get; set; }
}
