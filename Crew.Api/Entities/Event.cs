using System;
using System.Collections.Generic;
using Crew.Api.Models;

namespace Crew.Api.Entities;

public abstract class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OrganizerUid { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = EventStatuses.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }

    public UserAccount? Organizer { get; set; }
}

public static class EventStatuses
{
    public const string Draft = "draft";
    public const string Planning = "planning";
    public const string Published = "published";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Planning,
        Published,
        Completed,
        Cancelled,
    };

    public static bool IsValid(string? value)
        => !string.IsNullOrWhiteSpace(value) && All.Contains(value.Trim());
}
