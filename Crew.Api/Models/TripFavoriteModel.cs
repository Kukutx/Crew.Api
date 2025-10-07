using System;

namespace Crew.Api.Models;

public class TripFavoriteModel
{
    public int TripId { get; set; }
    public string UserUid { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
