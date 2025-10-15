using System.Security.Claims;
using Crew.Api.Mapping;
using Crew.Application.Events;
using Crew.Contracts.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crew.Api.Controllers;

[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
    private readonly IEventReadService _eventReadService;
    private readonly RegisterForEventCommand _registerForEventCommand;

    public EventsController(IEventReadService eventReadService, RegisterForEventCommand registerForEventCommand)
    {
        _eventReadService = eventReadService;
        _registerForEventCommand = registerForEventCommand;
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
}
