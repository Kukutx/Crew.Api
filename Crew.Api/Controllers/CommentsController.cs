using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Crew.Api.Data.DbContexts;
using Crew.Api.Models;
using Crew.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/events/{eventId}/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public CommentsController(AppDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Comment>>> GetAll(int eventId, CancellationToken cancellationToken)
    {
        var comments = await _context.Comments
            .Where(c => c.EventId == eventId)
            .ToListAsync(cancellationToken);

        return Ok(comments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Comment>> GetById(int eventId, int id, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.EventId == eventId && c.Id == id, cancellationToken);
        if (comment == null) return NotFound();
        return Ok(comment);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Comment>> Create(int eventId, Comment newComment, CancellationToken cancellationToken)
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

        if (!await _context.Events.AnyAsync(e => e.Id == eventId, cancellationToken)) return NotFound("Event not found");
        newComment.EventId = eventId;
        newComment.UserUid = currentUid;
        newComment.Content = newComment.Content.Trim();
        newComment.CreatedAt = DateTime.UtcNow;
        _context.Comments.Add(newComment);
        await _context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { eventId, id = newComment.Id }, newComment);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int eventId, int id, Comment updatedComment, CancellationToken cancellationToken)
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

        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.EventId == eventId && c.Id == id, cancellationToken);
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
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int eventId, int id, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (currentUid is null)
        {
            return Forbid();
        }

        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.EventId == eventId && c.Id == id, cancellationToken);
        if (comment == null) return NotFound();

        if (!string.Equals(comment.UserUid, currentUid, StringComparison.Ordinal))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, AuthorizationPolicies.RequireAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private string? GetCurrentUserUid()
        => User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}
