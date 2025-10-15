using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Crew.Api.Entities;

namespace Crew.Api.Models;

public class EventRegistration
{
    public int EventId { get; set; }
    public string UserUid { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public EventRegistrationStatus Status { get; set; } = EventRegistrationStatus.Pending;
    public DateTime StatusUpdatedAt { get; set; } = DateTime.UtcNow;

    public Event? Event { get; set; }
    public UserAccount? User { get; set; }
}

/// <summary>
/// Describes the processing status of an event registration.
/// </summary>
public enum EventRegistrationStatus
{
    /// <summary>
    /// The registration has been received and is awaiting confirmation.
    /// </summary>
    [EnumMember(Value = "pending")]
    Pending,

    /// <summary>
    /// The registration has been approved and the participant is confirmed.
    /// </summary>
    [EnumMember(Value = "confirmed")]
    Confirmed,
}
