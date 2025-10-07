using Crew.Api.Data;
using Crew.Api.Data.DbContexts;
using Crew.Api.Models;
using Crew.Api.Security;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserAccountResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.Role)
            .Include(u => u.Subscriptions)
                .ThenInclude(s => s.Plan)
            .OrderBy(u => u.UserName)
            .ToListAsync(cancellationToken);

        return Ok(users.Select(MapToResponse));
    }

    [HttpGet("{uid}")]
    [Authorize]
    public async Task<ActionResult<UserAccountResponse>> GetByUid(string uid, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.Role)
            .Include(u => u.Subscriptions)
                .ThenInclude(s => s.Plan)
            .FirstOrDefaultAsync(u => u.Uid == uid, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(user));
    }

    [HttpPost("ensure")]
    [AllowAnonymous]
    public async Task<ActionResult<UserAccountResponse>> EnsureUser([FromBody] EnsureUserRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("request body is required");
        }

        if (string.IsNullOrWhiteSpace(request.Uid))
        {
            return BadRequest("uid is required");
        }

        if (!TryNormalizeIdentityLabel(request.IdentityLabel, out var normalizedIdentityLabel, out var identityLabelError))
        {
            return BadRequest(identityLabelError);
        }

        var normalizedUid = request.Uid.Trim();
        var user = await _context.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.Role)
            .Include(u => u.Subscriptions)
                .ThenInclude(s => s.Plan)
            .FirstOrDefaultAsync(u => u.Uid == normalizedUid, cancellationToken);

        if (user is null)
        {
            var email = request.Email?.Trim() ?? string.Empty;
            var userName = request.UserName?.Trim();
            if (string.IsNullOrEmpty(userName))
            {
                userName = string.IsNullOrEmpty(email) ? normalizedUid : email;
            }

            var displayName = request.DisplayName?.Trim();
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = userName;
            }

            user = new UserAccount
            {
                Uid = normalizedUid,
                Email = email,
                UserName = userName,
                DisplayName = displayName,
                AvatarUrl = AvatarDefaults.Normalize(request.AvatarUrl),
                CreatedAt = DateTime.UtcNow,
                Status = UserStatuses.Active,
                IdentityLabel = normalizedIdentityLabel ?? UserIdentityLabels.Visitor,
            };

            await ApplyDefaultRoleAsync(user, cancellationToken);
            await ApplyDefaultPlanAsync(user, cancellationToken);

            _context.Users.Add(user);
        }
        else
        {
            var email = request.Email?.Trim();
            if (!string.IsNullOrEmpty(email))
            {
                user.Email = email;
            }

            var userName = request.UserName?.Trim();
            if (!string.IsNullOrEmpty(userName))
            {
                user.UserName = userName;
            }

            var displayName = request.DisplayName?.Trim();
            if (!string.IsNullOrEmpty(displayName))
            {
                user.DisplayName = displayName;
            }

            user.AvatarUrl = AvatarDefaults.Normalize(string.IsNullOrWhiteSpace(request.AvatarUrl)
                ? user.AvatarUrl
                : request.AvatarUrl);

            if (!string.IsNullOrWhiteSpace(request.IdentityLabel) && normalizedIdentityLabel is not null)
            {
                user.IdentityLabel = normalizedIdentityLabel;
            }
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(MapToResponse(user));
    }

    [HttpDelete("{uid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> Delete(string uid, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(uid))
        {
            return BadRequest("uid is required");
        }

        var user = await _context.Users
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .FirstOrDefaultAsync(u => u.Uid == uid, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        if (user.Followers.Any())
        {
            _context.UserFollows.RemoveRange(user.Followers);
        }

        if (user.Following.Any())
        {
            _context.UserFollows.RemoveRange(user.Following);
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task ApplyDefaultRoleAsync(UserAccount user, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Key == RoleKeys.User, cancellationToken);
        if (role == null)
        {
            throw new InvalidOperationException("Default user role is missing.");
        }

        user.Roles.Add(new UserRoleAssignment
        {
            RoleId = role.Id,
            UserUid = user.Uid,
            GrantedAt = DateTime.UtcNow,
        });
    }

    private async Task ApplyDefaultPlanAsync(UserAccount user, CancellationToken cancellationToken)
    {
        var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Key == SubscriptionPlanKeys.Free, cancellationToken);
        if (plan == null)
        {
            return;
        }

        user.Subscriptions.Add(new UserSubscription
        {
            PlanId = plan.Id,
            UserUid = user.Uid,
            AssignedAt = DateTime.UtcNow,
        });
    }

    private static UserAccountResponse MapToResponse(UserAccount user)
        => new(
            user.Uid,
            user.Email,
            user.UserName,
            user.DisplayName,
            user.AvatarUrl,
            user.CoverImageUrl,
            user.Status,
            user.IdentityLabel,
            user.CreatedAt,
            user.UpdatedAt,
            user.Roles
                .Where(r => r.Role != null)
                .Select(r => new RoleAssignmentResponse(r.Role!.Key, r.Role.DisplayName, r.GrantedAt))
                .OrderBy(r => r.Key)
                .ToList(),
            user.Subscriptions
                .Where(s => s.Plan != null)
                .Select(s => new SubscriptionResponse(s.Plan!.Key, s.Plan.DisplayName, s.AssignedAt, s.ExpiresAt))
                .OrderBy(s => s.PlanKey)
                .ToList());

    public record EnsureUserRequest(
        string Uid,
        string? Email,
        string? UserName,
        string? DisplayName,
        string? AvatarUrl,
        string? IdentityLabel = null);

    public record UserAccountResponse(
        string Uid,
        string Email,
        string UserName,
        string DisplayName,
        string AvatarUrl,
        string CoverImageUrl,
        string Status,
        string IdentityLabel,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        IReadOnlyCollection<RoleAssignmentResponse> Roles,
        IReadOnlyCollection<SubscriptionResponse> Subscriptions);

    public record RoleAssignmentResponse(string Key, string DisplayName, DateTime GrantedAt);

    public record SubscriptionResponse(string PlanKey, string PlanName, DateTime AssignedAt, DateTime? ExpiresAt);

    private static bool TryNormalizeIdentityLabel(string? identityLabel, out string? normalized, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(identityLabel))
        {
            normalized = null;
            errorMessage = null;
            return true;
        }

        var trimmed = identityLabel.Trim();
        if (!UserIdentityLabels.All.Contains(trimmed))
        {
            normalized = null;
            errorMessage = $"identity label must be one of: {string.Join(" / ", UserIdentityLabels.All)}.";
            return false;
        }

        normalized = trimmed;
        errorMessage = null;
        return true;
    }
}
