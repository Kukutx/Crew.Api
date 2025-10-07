using System;

namespace Crew.Api.Entities;

public class TripParticipant
{
    public int TripId { get; set; }
    public string UserUid { get; set; } = string.Empty;
    public string Role { get; set; } = TripParticipantRoles.Passenger;
    public DateTime JoinTime { get; set; }
    public string Status { get; set; } = TripParticipantStatuses.Pending;

    public Trip? Trip { get; set; }
    public Models.UserAccount? User { get; set; }
}

public static class TripParticipantRoles
{
    public const string Organizer = "Organizer";
    public const string Driver = "Driver";
    public const string Passenger = "Passenger";
    public const string Guest = "Guest";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Organizer,
        Driver,
        Passenger,
        Guest,
    };

    public static bool IsValid(string? value)
        => !string.IsNullOrWhiteSpace(value) && All.Contains(value.Trim());
}

public static class TripParticipantStatuses
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string Declined = "declined";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
        Confirmed,
        Declined,
    };

    public static bool IsValid(string? value)
        => !string.IsNullOrWhiteSpace(value) && All.Contains(value.Trim());
}
