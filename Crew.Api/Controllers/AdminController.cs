using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Crew.Application.Users;
using Crew.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crew.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = nameof(UserRole.Admin))]
public sealed class AdminController : ControllerBase
{
    private readonly IUserAdministrationService _administrationService;

    public AdminController(IUserAdministrationService administrationService)
    {
        _administrationService = administrationService;
    }

    [HttpPost("users/{firebaseUid}/make-admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PromoteToAdmin(string firebaseUid, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            return BadRequest();
        }

        var currentFirebaseUid = GetCurrentFirebaseUid();
        if (currentFirebaseUid is null)
        {
            return Unauthorized();
        }

        var result = await _administrationService.SetRoleAsync(firebaseUid, UserRole.Admin, currentFirebaseUid, cancellationToken);

        return result switch
        {
            SetUserRoleResult.Success => NoContent(),
            SetUserRoleResult.NotFound => NotFound(),
            SetUserRoleResult.Forbidden => Forbid(),
            _ => Forbid()
        };
    }

    private string? GetCurrentFirebaseUid()
    {
        var value = User.FindFirstValue("firebase_uid");
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
