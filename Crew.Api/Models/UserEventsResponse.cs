using System.Collections.Generic;

namespace Crew.Api.Models;

public record UserEventsResponse(
    string UserUid,
    IReadOnlyList<EventModal> Created,
    IReadOnlyList<EventModal> Favorited);
