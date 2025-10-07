using System;
using Crew.Api.Entities;

namespace Crew.Api.Models;

public class TripParticipantModel
{
    public int TripId { get; set; }
    public string UserUid { get; set; } = string.Empty;
    public DateTime JoinTime { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = TripParticipantStatuses.Pending;
    public string Role { get; set; } = TripParticipantRoles.Passenger;
}
