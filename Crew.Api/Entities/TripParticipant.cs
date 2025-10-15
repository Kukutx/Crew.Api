using System;
using System.Text.Json.Serialization;
using Crew.Api.Models;

namespace Crew.Api.Entities;

public class TripParticipant
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public string UserUid { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TripParticipantRole Role { get; set; } = TripParticipantRole.Passenger;
    public DateTime JoinTime { get; set; } = DateTime.UtcNow;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TripParticipantStatus Status { get; set; } = TripParticipantStatus.Pending;

    public Trip? Trip { get; set; }
    public UserAccount? User { get; set; }
}

/// <summary>
/// Defines the possible roles that a participant can assume in a trip.
/// </summary>
public enum TripParticipantRole
{
    /// <summary>
    /// The participant who organizes and manages the trip.
    /// </summary>
    Organizer,

    /// <summary>
    /// The participant responsible for driving during the trip.
    /// </summary>
    Driver,

    /// <summary>
    /// A standard participant who joins the trip as a passenger.
    /// </summary>
    Passenger,

    /// <summary>
    /// A guest participant with limited responsibilities in the trip.
    /// </summary>
    Guest,
}

/// <summary>
/// Represents the lifecycle statuses a trip participant can be in.
/// </summary>
public enum TripParticipantStatus
{
    /// <summary>
    /// The participant has requested to join and is awaiting confirmation.
    /// </summary>
    Pending,

    /// <summary>
    /// The participant has been approved to join the trip.
    /// </summary>
    Confirmed,

    /// <summary>
    /// The participant's request to join the trip was rejected.
    /// </summary>
    Rejected,

    /// <summary>
    /// The participant's involvement in the trip has been cancelled.
    /// </summary>
    Cancelled,
}
