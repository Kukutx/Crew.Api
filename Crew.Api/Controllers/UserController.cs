using System.Security.Claims;
using Crew.Api.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Crew.Api.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class UserController
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<LoginDetail> GetAuthenticatedUserDetail()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("No HttpContext available.");
        }

        var claimsPrincipal = httpContext.User;
        var firebaseId = claimsPrincipal.Claims.First(x => x.Type == "user_id").Value;
        var email = claimsPrincipal.Claims.First(x => x.Type == ClaimTypes.Email).Value;

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new InvalidOperationException($"No user found with email '{email}'.");
        }

        return new()
        {
            FirebaseId = firebaseId,
            Email = email,
            AspNetIdentityId = user.Id,
            RespondedAt = DateTime.Now,
        };
    }
}