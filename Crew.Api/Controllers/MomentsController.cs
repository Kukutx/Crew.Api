using System;
using System.Linq;
using System.Security.Claims;
using Crew.Api.Mapping;
using Asp.Versioning;
using Crew.Application.Moments;
using Crew.Contracts.Moments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crew.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/moments")]
public sealed class MomentsController : ControllerBase
{
    private readonly IMomentService _momentService;

    public MomentsController(IMomentService momentService)
    {
        _momentService = momentService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<MomentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAsync([FromQuery] string? country, [FromQuery] string? city, CancellationToken cancellationToken)
    {
        var moments = await _momentService.SearchAsync(country, city, cancellationToken);
        return Ok(moments.Select(m => m.ToDto()).ToList());
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MomentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var moment = await _momentService.GetAsync(id, cancellationToken);
        if (moment is null)
        {
            return NotFound();
        }

        return Ok(moment.ToDto());
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(MomentDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateMomentRequestDto request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var moment = await _momentService.CreateAsync(
            new CreateMomentRequest(
                userId.Value,
                request.EventId,
                request.Title,
                request.Content,
                request.CoverImageUrl,
                request.Country,
                request.City,
                request.Images ?? Array.Empty<string>()),
            cancellationToken);

        return CreatedAtAction(nameof(GetAsync), new { id = moment.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0" }, moment.ToDto());
    }

    [HttpPost("{id:guid}/comments")]
    [Authorize]
    [ProducesResponseType(typeof(MomentCommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddCommentAsync(Guid id, [FromBody] AddMomentCommentRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var comment = await _momentService.AddCommentAsync(id, userId.Value, request.Content, cancellationToken);
        return Ok(comment.ToDto());
    }

    [HttpGet("~/api/v{version:apiVersion}/users/{userId:guid}/moments")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<MomentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserMomentsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var moments = await _momentService.GetUserMomentsAsync(userId, cancellationToken);
        return Ok(moments.Select(m => m.ToDto()).ToList());
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
