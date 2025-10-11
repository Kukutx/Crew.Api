namespace Crew.Api.Models.Google;

public class NearbyRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusMeters { get; set; } = 50;
    public int MaxResultCount { get; set; } = 10;
    // 可选：限定类型（不传则全部）
    public List<string>? IncludedTypes { get; set; } = new() { "point_of_interest" };
    // 可选：字段掩码（默认给常用字段）
    public string? FieldMask { get; set; } =
        "places.name,places.displayName,places.formattedAddress,places.location,places.photos";
}
