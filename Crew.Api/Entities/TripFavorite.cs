using System;
using Crew.Api.Models;

namespace Crew.Api.Entities;

public class TripFavorite
{
    public int TripId { get; set; }
    public string UserUid { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Trip? Trip { get; set; }
    public UserAccount? User { get; set; }
}
