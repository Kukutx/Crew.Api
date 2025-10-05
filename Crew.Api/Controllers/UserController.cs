using System.Security.Claims;
using System.Threading;
using Crew.Api.Data.DbContexts;
using Crew.Api.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public UserController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<LoginDetail>> GetAuthenticatedUserDetail()
    {
        var claimsPrincipal = User;
        if (claimsPrincipal == null)
        {
            return Unauthorized();
        }

        var firebaseId = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
        var email = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(firebaseId) && string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        var cancellationToken = HttpContext?.RequestAborted ?? CancellationToken.None;

        var user = !string.IsNullOrWhiteSpace(firebaseId)
            ? await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Uid == firebaseId, cancellationToken)
            : null;

        if (user == null && !string.IsNullOrWhiteSpace(email))
        {
            user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        if (user == null)
        {
            return NotFound();
        }

        return new LoginDetail
        {
            FirebaseId = firebaseId ?? user.Uid,
            Email = user.Email,
            AspNetIdentityId = user.Uid,
            RespondedAt = DateTime.Now,
        };
    }
}
