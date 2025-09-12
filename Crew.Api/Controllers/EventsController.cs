using Crew.Api.Data;
using Crew.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly EventsDbContext _context;

    public EventsController(EventsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetAll()
        => Ok(await _context.Events.ToListAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetById(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();
        return Ok(ev);
    }

    [HttpPost]
    public async Task<ActionResult<Event>> Create(Event newEvent)
    {
        newEvent.Id = _context.Events.Any() ? _context.Events.Max(e => e.Id) + 1 : 1;
        if (string.IsNullOrEmpty(newEvent.CoverImageUrl) && newEvent.ImageUrls.Any())
        {
            newEvent.CoverImageUrl = newEvent.ImageUrls[0];
        }
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = newEvent.Id }, newEvent);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Event updatedEvent)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();

        ev.Title = updatedEvent.Title;
        ev.Location = updatedEvent.Location;
        ev.Description = updatedEvent.Description;
        ev.Latitude = updatedEvent.Latitude;
        ev.Longitude = updatedEvent.Longitude;
        ev.ImageUrls = updatedEvent.ImageUrls;
        ev.CoverImageUrl = string.IsNullOrEmpty(updatedEvent.CoverImageUrl) && updatedEvent.ImageUrls.Any()
            ? updatedEvent.ImageUrls[0]
            : updatedEvent.CoverImageUrl;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Event>>> SearchEvents(string query, double? lat, double? lng, double? radiusKm = 10)
    {
        var events = _context.Events.AsQueryable();
        if (!string.IsNullOrEmpty(query))
        {
            events = events.Where(e => e.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(await events.ToListAsync());
    }
}
