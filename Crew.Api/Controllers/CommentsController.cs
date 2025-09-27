using Crew.Api.Data;
using Crew.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/events/{eventId}/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CommentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Comment>>> GetAll(int eventId)
        => Ok(await _context.Comments.Where(c => c.EventId == eventId).ToListAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Comment>> GetById(int eventId, int id)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.EventId == eventId && c.Id == id);
        if (comment == null) return NotFound();
        return Ok(comment);
    }

    [HttpPost]
    public async Task<ActionResult<Comment>> Create(int eventId, Comment newComment)
    {
        if (!_context.Events.Any(e => e.Id == eventId)) return NotFound("Event not found");
        newComment.Id = _context.Comments.Any() ? _context.Comments.Max(c => c.Id) + 1 : 1;
        newComment.EventId = eventId;
        newComment.CreatedAt = DateTime.UtcNow;
        _context.Comments.Add(newComment);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { eventId, id = newComment.Id }, newComment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int eventId, int id, Comment updatedComment)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.EventId == eventId && c.Id == id);
        if (comment == null) return NotFound();
        comment.Content = updatedComment.Content;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int eventId, int id)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.EventId == eventId && c.Id == id);
        if (comment == null) return NotFound();
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
