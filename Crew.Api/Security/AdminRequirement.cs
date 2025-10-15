using System.Security.Claims;
using Crew.Api.Data;
using Crew.Api.Data.DbContexts;
using Crew.Api.Extensions;
using Crew.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Security;

public class AdminRequirement : IAuthorizationRequirement;

public class AdminRequirementHandler : AuthorizationHandler<AdminRequirement>
{
    private readonly AppDbContext _dbContext;

    public AdminRequirementHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        var uid = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(uid))
        {
            return;
        }

        var isAdmin = await _dbContext.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserUid == uid && ur.Role != null && ur.Role.Key == RoleKey.Admin.GetEnumMemberValue());

        if (isAdmin)
        {
            context.Succeed(requirement);
        }
    }
}
