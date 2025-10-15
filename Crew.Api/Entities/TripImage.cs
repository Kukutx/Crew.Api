using System;
using Crew.Api.Models;

namespace Crew.Api.Entities;

public class TripImage
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string UploaderUid { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Trip? Trip { get; set; }
    public UserAccount? Uploader { get; set; }
}
