using System.Collections.Generic;
using System.Threading;
using Crew.Api.Models.RemoteConfig;
using Crew.Api.Security;
using Crew.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/remote-config/disclaimers")]
[Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
public class RemoteConfigDisclaimersController : ControllerBase
{
    private readonly IFirebaseAdminService _firebaseAdminService;
    private readonly ILogger<RemoteConfigDisclaimersController> _logger;

    public RemoteConfigDisclaimersController(
        IFirebaseAdminService firebaseAdminService,
        ILogger<RemoteConfigDisclaimersController> logger)
    {
        _firebaseAdminService = firebaseAdminService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RemoteConfigDisclaimerDto>>> GetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var disclaimers = await _firebaseAdminService.GetDisclaimersAsync(cancellationToken);
            return Ok(disclaimers);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to read remote config disclaimers.");
            return Problem(ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RemoteConfigDisclaimerDto>> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            var disclaimer = await _firebaseAdminService.GetDisclaimerAsync(id, cancellationToken);
            if (disclaimer is null)
            {
                return NotFound();
            }

            return Ok(disclaimer);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to read remote config disclaimer {DisclaimerId}.", id);
            return Problem(ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    [HttpPost]
    public async Task<ActionResult<RemoteConfigDisclaimerDto>> CreateAsync(
        [FromBody] CreateRemoteConfigDisclaimerRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            ModelState.AddModelError(nameof(request.Message), "Message cannot be empty or whitespace.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var disclaimer = await _firebaseAdminService.CreateDisclaimerAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = disclaimer.Id }, disclaimer);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create remote config disclaimer.");
            return Problem(ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RemoteConfigDisclaimerDto>> UpdateAsync(
        string id,
        [FromBody] UpdateRemoteConfigDisclaimerRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            ModelState.AddModelError(nameof(request.Message), "Message cannot be empty or whitespace.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var disclaimer = await _firebaseAdminService.UpdateDisclaimerAsync(id, request, cancellationToken);
            return Ok(disclaimer);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to update remote config disclaimer {DisclaimerId}.", id);
            return Problem(ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _firebaseAdminService.DeleteDisclaimerAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to delete remote config disclaimer {DisclaimerId}.", id);
            return Problem(ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
