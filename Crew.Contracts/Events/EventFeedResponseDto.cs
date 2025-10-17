using System.Collections.Generic;

namespace Crew.Contracts.Events;

public sealed record EventFeedResponseDto(IReadOnlyList<EventCardDto> Events, string? NextCursor);
