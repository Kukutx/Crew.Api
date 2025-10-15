namespace Crew.Contracts.Places;

public sealed record PlaceSummaryDto(string PlaceId, string Name, double[] Location, IReadOnlyList<string> Types);
