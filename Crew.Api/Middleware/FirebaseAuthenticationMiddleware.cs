using System;
using System.Security.Claims;
using Crew.Application.Auth;
using Crew.Domain.Enums;
using Serilog.Context;

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
        Guid? userId = null;

        var authorization = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authorization.Substring("Bearer ".Length).Trim();
            var result = await tokenVerifier.VerifyAsync(token, context.RequestAborted);
            if (result is not null)
            {
                UserRole? role = null;
                if (!string.IsNullOrWhiteSpace(result.Role) && Enum.TryParse<UserRole>(result.Role, true, out var parsedRole))
                {
                    role = parsedRole;
                }

                var user = await userProvisioningService.EnsureUserAsync(
                    result.FirebaseUid,
                    result.DisplayName,
                    result.Email,
                    role,
                    cancellationToken: context.RequestAborted);
                userId = user.Id;

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new("firebase_uid", result.FirebaseUid)
                };

                claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));

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

        using (LogContext.PushProperty("user_id", userId))
        {
            await _next(context);
        }
    }
}
