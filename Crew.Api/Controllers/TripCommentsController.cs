using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Crew.Api.Data.DbContexts;
using Crew.Api.Entities;
using Crew.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/trips/{tripId}/[controller]")]
public class TripCommentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public TripCommentsController(AppDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TripComment>>> GetAll(int tripId, CancellationToken cancellationToken)
    {
        var comments = await _context.TripComments
            .Where(c => c.TripId == tripId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(comments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TripComment>> GetById(int tripId, int id, CancellationToken cancellationToken)
    {
        var comment = await _context.TripComments
            .FirstOrDefaultAsync(c => c.TripId == tripId && c.Id == id, cancellationToken);
        if (comment == null) return NotFound();
        return Ok(comment);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TripComment>> Create(int tripId, TripComment newComment, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (currentUid is null)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(newComment.Content))
        {
            return BadRequest("Content is required.");
        }

        var tripExists = await _context.Trips.AnyAsync(t => t.Id == tripId, cancellationToken);
        if (!tripExists) return NotFound("Trip not found");

        newComment.TripId = tripId;
        newComment.UserUid = currentUid;
        newComment.Content = newComment.Content.Trim();
        newComment.CreatedAt = DateTime.UtcNow;
        _context.TripComments.Add(newComment);
        await _context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { tripId, id = newComment.Id }, newComment);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int tripId, int id, TripComment updatedComment, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (currentUid is null)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(updatedComment.Content))
        {
            return BadRequest("Content is required.");
        }

        var comment = await _context.TripComments
            .FirstOrDefaultAsync(c => c.TripId == tripId && c.Id == id, cancellationToken);
        if (comment == null) return NotFound();

        if (!string.Equals(comment.UserUid, currentUid, StringComparison.Ordinal))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, AuthorizationPolicies.RequireAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        comment.Content = updatedComment.Content.Trim();
        comment.Rating = updatedComment.Rating;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int tripId, int id, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (currentUid is null)
        {
            return Forbid();
        }

        var comment = await _context.TripComments
            .FirstOrDefaultAsync(c => c.TripId == tripId && c.Id == id, cancellationToken);
        if (comment == null) return NotFound();

        if (!string.Equals(comment.UserUid, currentUid, StringComparison.Ordinal))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, AuthorizationPolicies.RequireAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        _context.TripComments.Remove(comment);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private string? GetCurrentUserUid()
        => User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}
