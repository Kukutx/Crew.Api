using System;
using Crew.Api.Models;

namespace Crew.Api.Entities;

public class TripComment
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public string UserUid { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Trip? Trip { get; set; }
    public UserAccount? User { get; set; }
}
