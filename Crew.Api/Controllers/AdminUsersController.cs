using Crew.Api.Data;
using Crew.Api.Models;
using Crew.Api.Security;
using Crew.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IFirebaseAdminService _firebaseAdminService;

    public AdminUsersController(AppDbContext context, IFirebaseAdminService firebaseAdminService)
    {
        _context = context;
        _firebaseAdminService = firebaseAdminService;
    }

    [HttpPost("{uid}/set-admin")]
    public async Task<IActionResult> SetAdmin(string uid, [FromBody] SetAdminRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("request body is required");
        }

        if (string.IsNullOrWhiteSpace(uid))
        {
            return BadRequest("uid is required");
        }

        var user = await _context.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.Role)
            .FirstOrDefaultAsync(u => u.Uid == uid, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Key == RoleKeys.Admin, cancellationToken);
        if (adminRole is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "admin role is not configured");
        }

        var assignment = user.Roles.FirstOrDefault(r => r.RoleId == adminRole.Id);
        if (request.IsAdmin && assignment is null)
        {
            user.Roles.Add(new UserRoleAssignment
            {
                RoleId = adminRole.Id,
                UserUid = user.Uid,
                GrantedAt = DateTime.UtcNow,
            });
        }
        else if (!request.IsAdmin && assignment is not null)
        {
            _context.UserRoles.Remove(assignment);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _firebaseAdminService.SetAdminClaimAsync(user.Uid, request.IsAdmin, cancellationToken);

        return NoContent();
    }

    public record SetAdminRequest(bool IsAdmin);
}
