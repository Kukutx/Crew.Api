namespace Crew.Contracts.Places;

public sealed record PlaceDetailDto(string PlaceId, string Name, double[] Location, IReadOnlyList<string> Types, string? FormattedAddress);
