using System.Security.Claims;
using Crew.Application.Auth;

namespace Crew.Api.Middleware;

public sealed class FirebaseAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public FirebaseAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IFirebaseTokenVerifier tokenVerifier, UserProvisioningService userProvisioningService)
    {
        var authorization = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authorization.Substring("Bearer ".Length).Trim();
            var result = await tokenVerifier.VerifyAsync(token, context.RequestAborted);
            if (result is not null)
            {
                var user = await userProvisioningService.EnsureUserAsync(result.FirebaseUid, result.DisplayName, context.RequestAborted);
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new("firebase_uid", result.FirebaseUid)
                };

                if (!string.IsNullOrEmpty(result.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, result.Email));
                }

                if (!string.IsNullOrEmpty(result.DisplayName))
                {
                    claims.Add(new Claim(ClaimTypes.Name, result.DisplayName));
                }

                var identity = new ClaimsIdentity(claims, "Firebase");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await _next(context);
    }
}
