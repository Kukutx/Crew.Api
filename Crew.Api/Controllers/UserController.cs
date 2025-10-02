using System.Security.Claims;
using Crew.Api.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Crew.Api.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class UserController
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _dbContext;

    public UserController(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
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
        var firebaseId = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
        var email = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(firebaseId) && string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Authenticated user is missing Firebase UID and email claims.");
        }

        var cancellationToken = httpContext.RequestAborted;

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
            throw new InvalidOperationException("No user found for the authenticated principal.");
        }

        return new()
        {
            FirebaseId = firebaseId ?? user.Uid,
            Email = user.Email,
            AspNetIdentityId = user.Uid,
            RespondedAt = DateTime.Now,
        };
    }
}