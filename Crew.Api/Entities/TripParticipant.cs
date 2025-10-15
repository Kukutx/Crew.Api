using System;
using Crew.Api.Models;

namespace Crew.Api.Entities;

public class TripParticipant
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public string UserUid { get; set; } = string.Empty;
    public string Role { get; set; } = TripParticipantRoles.Passenger;
    public DateTime JoinTime { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = TripParticipantStatuses.Pending;

    public Trip? Trip { get; set; }
    public UserAccount? User { get; set; }
}

public static class TripParticipantRoles
{
    public const string Organizer = "Organizer";
    public const string Driver = "Driver";
    public const string Passenger = "Passenger";
    public const string Guest = "Guest";
}

public static class TripParticipantStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
}
