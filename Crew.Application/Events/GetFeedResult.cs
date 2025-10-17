using System.Collections.Generic;

namespace Crew.Application.Events;

public sealed record GetFeedResult(IReadOnlyList<EventCard> Events, string? NextCursor);
