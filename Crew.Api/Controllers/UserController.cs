using System.Security.Claims;
using Crew.Api.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Crew.Api.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(LoginDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginDetail>> GetAuthenticatedUserDetail()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        var firebaseId = user.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
        var email = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(firebaseId) || string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Required authentication claims were not found.");
        }

        var identityUser = await _userManager.FindByEmailAsync(email);
        if (identityUser is null)
        {
            return NotFound();
        }

        var loginDetail = new LoginDetail
        {
            FirebaseId = firebaseId,
            Email = email,
            AspNetIdentityId = identityUser.Id,
            RespondedAt = DateTime.UtcNow,
        };

        return Ok(loginDetail);
    }
}
