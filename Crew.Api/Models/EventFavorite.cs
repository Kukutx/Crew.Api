using System;
using Crew.Api.Entities;

namespace Crew.Api.Models;

public class EventFavorite
{
    public int EventId { get; set; }
    public string UserUid { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Event? Event { get; set; }
    public UserAccount? User { get; set; }
}
