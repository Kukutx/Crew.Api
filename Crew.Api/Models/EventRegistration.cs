using System;
using System.Collections.Generic;
using Crew.Api.Entities;

namespace Crew.Api.Models;

public class EventRegistration
{
    public int EventId { get; set; }
    public string UserUid { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = EventRegistrationStatuses.Pending;
    public DateTime StatusUpdatedAt { get; set; } = DateTime.UtcNow;

    public Event? Event { get; set; }
    public UserAccount? User { get; set; }
}

public static class EventRegistrationStatuses
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Pending,
        Confirmed,
    };

    public static bool IsValid(string? value)
        => value is not null &&
           (string.Equals(value, Pending, StringComparison.Ordinal) ||
            string.Equals(value, Confirmed, StringComparison.Ordinal));
}
