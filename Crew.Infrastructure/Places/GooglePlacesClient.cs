using System.Text.Json;
using Crew.Application.Places;
using Crew.Contracts.Places;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Crew.Infrastructure.Places;

internal sealed class GooglePlacesClient : IGooglePlacesClient
{
    private readonly HttpClient _httpClient;
    private readonly GooglePlacesOptions _options;

    public GooglePlacesClient(HttpClient httpClient, IOptions<GooglePlacesOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<PlaceSummaryDto>> FindTextAsync(string query, double? longitude, double? latitude, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Google Places API key is not configured.");
        }

        var parameters = new Dictionary<string, string>
        {
            ["key"] = _options.ApiKey!,
            ["query"] = query
        };

        if (longitude is not null && latitude is not null)
        {
            parameters["locationbias"] = $"point:{latitude.Value},{longitude.Value}";
        }

        var url = QueryString(_options.BaseUrl + "textsearch/json", parameters);
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<PlaceSummaryDto>();
        }

        var list = new List<PlaceSummaryDto>();
        foreach (var result in results.EnumerateArray())
        {
            var placeId = result.GetProperty("place_id").GetString();
            var name = result.GetProperty("name").GetString();
            var types = result.TryGetProperty("types", out var typesElement) && typesElement.ValueKind == JsonValueKind.Array
                ? typesElement.EnumerateArray().Select(x => x.GetString() ?? string.Empty).Where(x => x.Length > 0).ToList()
                : new List<string>();

            if (!result.TryGetProperty("geometry", out var geometry) || !geometry.TryGetProperty("location", out var location))
            {
                continue;
            }

            var lat = location.GetProperty("lat").GetDouble();
            var lng = location.GetProperty("lng").GetDouble();

            if (string.IsNullOrEmpty(placeId) || string.IsNullOrEmpty(name))
            {
                continue;
            }

            list.Add(new PlaceSummaryDto(placeId, name, new[] { lng, lat }, types));
        }

        return list;
    }

    public async Task<PlaceDetailDto?> GetDetailsAsync(string placeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Google Places API key is not configured.");
        }

        var parameters = new Dictionary<string, string>
        {
            ["key"] = _options.ApiKey!,
            ["place_id"] = placeId
        };

        var url = QueryString(_options.BaseUrl + "details/json", parameters);
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var name = result.GetProperty("name").GetString();
        var formattedAddress = result.TryGetProperty("formatted_address", out var addressElement) ? addressElement.GetString() : null;
        var types = result.TryGetProperty("types", out var typesElement) && typesElement.ValueKind == JsonValueKind.Array
            ? typesElement.EnumerateArray().Select(x => x.GetString() ?? string.Empty).Where(x => x.Length > 0).ToList()
            : new List<string>();

        if (!result.TryGetProperty("geometry", out var geometry) || !geometry.TryGetProperty("location", out var location))
        {
            return null;
        }

        var lat = location.GetProperty("lat").GetDouble();
        var lng = location.GetProperty("lng").GetDouble();

        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return new PlaceDetailDto(placeId, name, new[] { lng, lat }, types, formattedAddress);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"Google Places API error: {(int)response.StatusCode} {response.ReasonPhrase} - {body}");
    }

    private static string QueryString(string baseUrl, IReadOnlyDictionary<string, string> parameters)
    {
        var builder = new UriBuilder(baseUrl);
        var query = string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        builder.Query = query;
        return builder.ToString();
    }
}
