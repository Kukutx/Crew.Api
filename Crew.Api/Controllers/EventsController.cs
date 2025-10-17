using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Crew.Application.Events;
using Crew.Api.Mapping;
using Crew.Contracts.Events;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Crew.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/events")]
public class EventsController : ControllerBase
{
    private readonly IEventReadService _eventReadService;
    private readonly RegisterForEventCommand _registerForEventCommand;
    private readonly IGetFeedQueryHandler _getFeedQueryHandler;
    private readonly IValidator<GetFeedQuery> _getFeedQueryValidator;

    public EventsController(
        IEventReadService eventReadService,
        RegisterForEventCommand registerForEventCommand,
        IGetFeedQueryHandler getFeedQueryHandler,
        IValidator<GetFeedQuery> getFeedQueryValidator)
    {
        _eventReadService = eventReadService;
        _registerForEventCommand = registerForEventCommand;
        _getFeedQueryHandler = getFeedQueryHandler;
        _getFeedQueryValidator = getFeedQueryValidator;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<EventSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAsync([FromQuery] double? minLng, [FromQuery] double? minLat, [FromQuery] double? maxLng, [FromQuery] double? maxLat, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var request = new EventSearchRequest(minLng, minLat, maxLng, maxLat, from, to, q);
        var events = await _eventReadService.SearchAsync(request, userId, cancellationToken);
        return Ok(events.Select(e => e.ToDto()).ToList());
    }

    [HttpGet("feed")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventFeedResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async Task<IActionResult> GetFeedAsync(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radius,
        [FromQuery] int limit = 20,
        [FromQuery] string? cursor = null,
        [FromQuery(Name = "tags")] string[]? tags = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFeedQuery(lat, lng, radius, limit, cursor, tags);
        var validation = await _getFeedQueryValidator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var result = await _getFeedQueryHandler.HandleAsync(query, cancellationToken);
        var cacheKey = BuildCacheKey(query, result);
        var etag = GenerateETag(cacheKey);

        if (Request.Headers.TryGetValue("If-None-Match", out StringValues existing) &&
            existing.SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(value => value.Trim())
                .Any(value => string.Equals(value, etag, StringComparison.Ordinal)))
        {
            Response.Headers.ETag = etag;
            Response.Headers.CacheControl = "public,max-age=60";
            return StatusCode(StatusCodes.Status304NotModified);
        }

        var payload = new EventFeedResponseDto(result.Events.Select(e => e.ToDto()).ToList(), result.NextCursor);

        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "public,max-age=60";

        return Ok(payload);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var detail = await _eventReadService.GetDetailAsync(id, userId, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        return Ok(detail.ToDto());
    }

    [HttpPost("{id:guid}/registrations")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        await _registerForEventCommand.RegisterAsync(id, userId.Value, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/registrations/me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        await _registerForEventCommand.CancelAsync(id, userId.Value, cancellationToken);
        return NoContent();
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static string GenerateETag(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return $"\"{Convert.ToHexString(bytes)}\"";
    }

    private static string BuildCacheKey(GetFeedQuery query, GetFeedResult result)
    {
        var tags = query.Tags?.Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase) ?? Enumerable.Empty<string>();

        var fingerprints = result.Events.Count == 0
            ? "empty"
            : string.Join(';', result.Events.Select(e => string.Join(':',
                e.Id.ToString("N", CultureInfo.InvariantCulture),
                e.CreatedAt.UtcTicks.ToString(CultureInfo.InvariantCulture),
                e.LastModified.UtcTicks.ToString(CultureInfo.InvariantCulture),
                e.DistanceKm.ToString("F6", CultureInfo.InvariantCulture),
                e.Registrations.ToString(CultureInfo.InvariantCulture),
                e.Likes.ToString(CultureInfo.InvariantCulture),
                e.Engagement.ToString("F6", CultureInfo.InvariantCulture))));

        return string.Join(
            '|',
            query.Latitude.ToString("F6", CultureInfo.InvariantCulture),
            query.Longitude.ToString("F6", CultureInfo.InvariantCulture),
            query.RadiusKm.ToString("F2", CultureInfo.InvariantCulture),
            query.Limit.ToString(CultureInfo.InvariantCulture),
            query.Cursor ?? string.Empty,
            string.Join(',', tags),
            result.NextCursor ?? string.Empty,
            fingerprints);
    }
}
