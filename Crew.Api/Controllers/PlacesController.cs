using Crew.Application.Places;
using Crew.Contracts.Places;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crew.Api.Controllers;

[ApiController]
[Route("places")]
public sealed class PlacesController : ControllerBase
{
    private readonly IGooglePlacesClient _placesClient;

    public PlacesController(IGooglePlacesClient placesClient)
    {
        _placesClient = placesClient;
    }

    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<PlaceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAsync([FromQuery] string q, [FromQuery] double? lng, [FromQuery] double? lat, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Query is required",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var results = await _placesClient.FindTextAsync(q, lng, lat, cancellationToken);
        return Ok(results);
    }

    [HttpGet("{placeId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PlaceDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(string placeId, CancellationToken cancellationToken)
    {
        var result = await _placesClient.GetDetailsAsync(placeId, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
