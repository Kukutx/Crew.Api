namespace Crew.Infrastructure.Places;

public sealed class GooglePlacesOptions
{
    public const string SectionName = "Google:Places";
    public string? ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://maps.googleapis.com/maps/api/place/";
}
