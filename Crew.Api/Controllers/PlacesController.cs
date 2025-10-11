using Crew.Api.Models.Google;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PlacesController : ControllerBase
{
    private readonly HttpClient _http;
    public PlacesController(IHttpClientFactory factory)
        => _http = factory.CreateClient("GooglePlaces");

    // POST: /api/places/nearby
    [HttpPost("nearby")]
    public async Task<IActionResult> Nearby([FromBody] NearbyRequest req, CancellationToken ct)
    {
        var body = new
        {
            includedTypes = req.IncludedTypes?.Count > 0 ? req.IncludedTypes : null,
            maxResultCount = req.MaxResultCount,
            locationRestriction = new
            {
                circle = new
                {
                    center = new { latitude = req.Latitude, longitude = req.Longitude },
                    radius = req.RadiusMeters
                }
            }
        };

        using var msg = new HttpRequestMessage(HttpMethod.Post, "places:searchNearby")
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(body),
                                        System.Text.Encoding.UTF8, "application/json")
        };
        msg.Headers.TryAddWithoutValidation("X-Goog-FieldMask",
            string.IsNullOrWhiteSpace(req.FieldMask) ? "places.name" : req.FieldMask);

        using var res = await _http.SendAsync(msg, ct);
        var content = await res.Content.ReadAsStringAsync(ct);
        return StatusCode((int)res.StatusCode, content);
    }

    // GET: /api/places/{placeId}?fieldMask=...
    [HttpGet("{placeId}")]
    public async Task<IActionResult> Details([FromRoute] string placeId, [FromQuery] string? fieldMask, CancellationToken ct)
    {
        var resource = placeId.StartsWith("places/") ? placeId : $"places/{placeId}";
        using var msg = new HttpRequestMessage(HttpMethod.Get, resource);
        msg.Headers.TryAddWithoutValidation("X-Goog-FieldMask",
            string.IsNullOrWhiteSpace(fieldMask)
                ? "name,displayName,formattedAddress,location,rating,userRatingCount,priceLevel"
                : fieldMask);

        using var res = await _http.SendAsync(msg, ct);
        var content = await res.Content.ReadAsStringAsync(ct);
        return StatusCode((int)res.StatusCode, content);
    }

    // GET: /api/places/photo?name=places/XXX/photos/YYY&maxHeightPx=360
    [HttpGet("photo")]
    public async Task<IActionResult> Photo([FromQuery] string name, [FromQuery] int? maxHeightPx, [FromQuery] int? maxWidthPx, CancellationToken ct)
    {
        var qs = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (maxHeightPx is > 0) qs["maxHeightPx"] = maxHeightPx.ToString();
        if (maxWidthPx is > 0) qs["maxWidthPx"] = maxWidthPx.ToString();

        var url = $"https://places.googleapis.com/v1/{name}/media";
        if (qs.Count > 0) url += "?" + qs.ToString();

        // 直连媒体（需要 key 作为 query ?key=）
        var mediaReq = new HttpRequestMessage(HttpMethod.Get, $"{url}&key={_http.DefaultRequestHeaders.GetValues("X-Goog-Api-Key").First()}");
        mediaReq.Headers.Accept.Clear();
        mediaReq.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

        var res = await _http.SendAsync(mediaReq, HttpCompletionOption.ResponseHeadersRead, ct);
        var stream = await res.Content.ReadAsStreamAsync(ct);
        var contentType = res.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
        return File(stream, contentType);
    }
}
