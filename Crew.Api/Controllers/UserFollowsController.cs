using System;
using System.Linq;
using Crew.Api.Data.DbContexts;
using Crew.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/users/{userUid}")]
public class UserFollowsController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserFollowsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("followers")]
    public async Task<ActionResult<IEnumerable<UserSummary>>> GetFollowers(string userUid, CancellationToken cancellationToken)
    {
        if (!await _context.Users.AsNoTracking().AnyAsync(u => u.Uid == userUid, cancellationToken))
        {
            return NotFound();
        }

        var followers = await _context.UserFollows
            .AsNoTracking()
            .Where(f => f.FollowedUid == userUid)
            .Join(
                _context.Users.AsNoTracking(),
                follow => follow.FollowerUid,
                user => user.Uid,
                (follow, user) => new { follow.CreatedAt, User = user })
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapToSummary(x.User))
            .ToListAsync(cancellationToken);

        return Ok(followers);
    }

    [HttpGet("following")]
    public async Task<ActionResult<IEnumerable<UserSummary>>> GetFollowing(string userUid, CancellationToken cancellationToken)
    {
        if (!await _context.Users.AsNoTracking().AnyAsync(u => u.Uid == userUid, cancellationToken))
        {
            return NotFound();
        }

        var following = await _context.UserFollows
            .AsNoTracking()
            .Where(f => f.FollowerUid == userUid)
            .Join(
                _context.Users.AsNoTracking(),
                follow => follow.FollowedUid,
                user => user.Uid,
                (follow, user) => new { follow.CreatedAt, User = user })
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapToSummary(x.User))
            .ToListAsync(cancellationToken);

        return Ok(following);
    }

    [HttpPost("following")]
    public async Task<ActionResult<UserSummary>> FollowUser(
        string userUid,
        [FromBody] FollowRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.TargetUid))
        {
            return BadRequest("targetUid is required");
        }

        var followerUid = userUid.Trim();
        var targetUid = request.TargetUid.Trim();

        if (string.Equals(followerUid, targetUid, StringComparison.Ordinal))
        {
            return BadRequest("Users cannot follow themselves.");
        }

        var follower = await _context.Users.FirstOrDefaultAsync(u => u.Uid == followerUid, cancellationToken);
        if (follower is null)
        {
            return NotFound($"User '{followerUid}' not found.");
        }

        var target = await _context.Users.FirstOrDefaultAsync(u => u.Uid == targetUid, cancellationToken);
        if (target is null)
        {
            return NotFound($"User '{targetUid}' not found.");
        }

        var existingFollow = await _context.UserFollows.FindAsync(new object[] { followerUid, targetUid }, cancellationToken);
        if (existingFollow is not null)
        {
            return Conflict("Follow relationship already exists.");
        }

        _context.UserFollows.Add(new UserFollow
        {
            FollowerUid = followerUid,
            FollowedUid = targetUid,
            CreatedAt = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetFollowing),
            new { userUid = followerUid },
            MapToSummary(target));
    }

    [HttpDelete("following/{targetUid}")]
    public async Task<IActionResult> UnfollowUser(string userUid, string targetUid, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(targetUid))
        {
            return BadRequest("targetUid is required");
        }

        var followerUid = userUid.Trim();
        var normalizedTargetUid = targetUid.Trim();

        var follow = await _context.UserFollows.FindAsync(new object[] { followerUid, normalizedTargetUid }, cancellationToken);
        if (follow is null)
        {
            return NotFound();
        }

        _context.UserFollows.Remove(follow);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static UserSummary MapToSummary(UserAccount user)
        => new(user.Uid, user.DisplayName, user.UserName, user.AvatarUrl);

    public record FollowRequest(string TargetUid);

    public record UserSummary(string Uid, string DisplayName, string UserName, string AvatarUrl);
}
