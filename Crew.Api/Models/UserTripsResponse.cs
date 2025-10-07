using System.Collections.Generic;

namespace Crew.Api.Models;

public record UserTripsResponse(
    IReadOnlyList<TripModal> Created,
    IReadOnlyList<TripModal> Favorited);
