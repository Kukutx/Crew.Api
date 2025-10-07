using System;

namespace Crew.Api.Entities;

public class TripSchedule
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public DateTime Date { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Hotel { get; set; } = string.Empty;
    public string Meal { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public Trip? Trip { get; set; }
}
