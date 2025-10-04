using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Crew.Api.Data.DbContexts;
using Crew.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/users/{uid}/[controller]")]
public class FollowersController : ControllerBase
{
    private readonly AppDbContext _context;

    public FollowersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserFollowSummary>>> GetFollowers(string uid, CancellationToken cancellationToken)
    {
        if (!await _context.Users.AnyAsync(u => u.Uid == uid, cancellationToken))
        {
            return NotFound();
        }

        var followers = await _context.UserFollows
            .AsNoTracking()
            .Where(f => f.FollowedUid == uid)
            .Include(f => f.Follower)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => MapToSummary(f.FollowerUid, f.Follower, f.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(followers);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> FollowUser(string uid, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (currentUid is null)
        {
            return Forbid();
        }

        if (currentUid == uid)
        {
            return BadRequest("Users cannot follow themselves.");
        }

        if (!await _context.Users.AnyAsync(u => u.Uid == uid, cancellationToken))
        {
            return NotFound();
        }

        var existingFollow = await _context.UserFollows.FindAsync(new object?[] { currentUid, uid }, cancellationToken);
        if (existingFollow != null)
        {
            return NoContent();
        }

        _context.UserFollows.Add(new UserFollow
        {
            FollowerUid = currentUid,
            FollowedUid = uid,
            CreatedAt = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> UnfollowUser(string uid, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (currentUid is null)
        {
            return Forbid();
        }

        var follow = await _context.UserFollows.FindAsync(new object?[] { currentUid, uid }, cancellationToken);
        if (follow == null)
        {
            return NoContent();
        }

        _context.UserFollows.Remove(follow);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private string? GetCurrentUserUid()
        => User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static UserFollowSummary MapToSummary(string uid, UserAccount? user, DateTime followedAt)
        => new(
            uid,
            user?.UserName ?? string.Empty,
            user?.DisplayName ?? string.Empty,
            user?.AvatarUrl ?? AvatarDefaults.FallbackUrl,
            followedAt);
}
