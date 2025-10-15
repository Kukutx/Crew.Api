using Crew.Contracts.Places;

namespace Crew.Application.Places;

public interface IGooglePlacesClient
{
    Task<IReadOnlyList<PlaceSummaryDto>> FindTextAsync(string query, double? longitude, double? latitude, CancellationToken cancellationToken = default);
    Task<PlaceDetailDto?> GetDetailsAsync(string placeId, CancellationToken cancellationToken = default);
}
