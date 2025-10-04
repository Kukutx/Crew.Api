using System;
using Crew.Api.Entities;

namespace Crew.Api.Models;

public class Comment
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string UserUid { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Event? Event { get; set; }
    public UserAccount? User { get; set; }
}
