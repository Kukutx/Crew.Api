using System.Text.Json.Serialization;

namespace Crew.Api.Models.RemoteConfig;

public record RemoteConfigDisclaimerDto
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("locale")]
    public string? Locale { get; init; }

    [JsonPropertyName("lastUpdatedUtc")]
    public DateTime LastUpdatedUtc { get; init; }
}
