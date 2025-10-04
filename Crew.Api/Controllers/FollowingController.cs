using System;
using System.Collections.Generic;
using System.Linq;
using Crew.Api.Data.DbContexts;
using Crew.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/users/{uid}/[controller]")]
public class FollowingController : ControllerBase
{
    private readonly AppDbContext _context;

    public FollowingController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserFollowSummary>>> GetFollowing(string uid, CancellationToken cancellationToken)
    {
        if (!await _context.Users.AnyAsync(u => u.Uid == uid, cancellationToken))
        {
            return NotFound();
        }

        var following = await _context.UserFollows
            .AsNoTracking()
            .Where(f => f.FollowerUid == uid)
            .Include(f => f.Followed)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => MapToSummary(f.FollowedUid, f.Followed, f.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(following);
    }

    private static UserFollowSummary MapToSummary(string uid, UserAccount? user, DateTime followedAt)
        => new(
            uid,
            user?.UserName ?? string.Empty,
            user?.DisplayName ?? string.Empty,
            user?.AvatarUrl ?? AvatarDefaults.FallbackUrl,
            followedAt);
}
