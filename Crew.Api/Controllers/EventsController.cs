using System;
using System.Collections.Generic;
using System.Linq;
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
        SanitizeEvent(newEvent);

        if (newEvent.StartTime == default)
        {
            newEvent.StartTime = DateTime.UtcNow;
        }

        if (newEvent.EndTime == default)
        {
            newEvent.EndTime = newEvent.StartTime;
        }

        if (newEvent.EndTime < newEvent.StartTime)
        {
            return BadRequest("End time cannot be earlier than the start time.");
        }

        newEvent.CreatedAt = DateTime.UtcNow;
        newEvent.LastUpdated = newEvent.CreatedAt;
        newEvent.ExpectedParticipants = Math.Max(0, newEvent.ExpectedParticipants);
        newEvent.ImageUrls = NormalizeImageUrls(newEvent.ImageUrls);
        newEvent.CoverImageUrl = ResolveCoverImage(newEvent.CoverImageUrl, newEvent.ImageUrls);

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = newEvent.Id }, newEvent);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Event updatedEvent)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();

        SanitizeEvent(updatedEvent);
        if (updatedEvent.StartTime != default && updatedEvent.EndTime != default &&
            updatedEvent.EndTime < updatedEvent.StartTime)
        {
            return BadRequest("End time cannot be earlier than the start time.");
        }

        ev.Title = updatedEvent.Title;
        ev.Type = updatedEvent.Type;
        ev.Status = updatedEvent.Status;
        ev.Organizer = updatedEvent.Organizer;
        ev.Location = updatedEvent.Location;
        ev.Description = updatedEvent.Description;
        ev.ExpectedParticipants = Math.Max(0, updatedEvent.ExpectedParticipants);
        if (updatedEvent.StartTime != default)
        {
            ev.StartTime = updatedEvent.StartTime;
        }
        if (updatedEvent.EndTime != default)
        {
            ev.EndTime = updatedEvent.EndTime;
        }
        ev.Latitude = updatedEvent.Latitude;
        ev.Longitude = updatedEvent.Longitude;
        ev.ImageUrls = NormalizeImageUrls(updatedEvent.ImageUrls);
        ev.CoverImageUrl = ResolveCoverImage(updatedEvent.CoverImageUrl, ev.ImageUrls);
        ev.LastUpdated = DateTime.UtcNow;

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
    public async Task<ActionResult<IEnumerable<Event>>> SearchEvents(
        string? query,
        double? lat,
        double? lng,
        double? radiusKm = 10,
        string? type = null,
        string? status = null)
    {
        var events = _context.Events.AsQueryable();
        if (!string.IsNullOrEmpty(query))
        {
            var likeQuery = $"%{query.Trim()}%";
            events = events.Where(e =>
                EF.Functions.Like(e.Title, likeQuery) ||
                EF.Functions.Like(e.Description, likeQuery) ||
                EF.Functions.Like(e.Location, likeQuery));
        }

        var result = await events.ToListAsync();

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = type.Trim();
            result = result
                .Where(e => !string.IsNullOrEmpty(e.Type) &&
                            string.Equals(e.Type, normalizedType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim();
            result = result
                .Where(e => !string.IsNullOrEmpty(e.Status) &&
                            string.Equals(e.Status, normalizedStatus, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (lat.HasValue && lng.HasValue && radiusKm.HasValue && radiusKm.Value > 0)
        {
            var originLat = lat.Value;
            var originLng = lng.Value;
            var radius = radiusKm.Value;

            result = result
                .Where(e => CalculateDistanceKm(originLat, originLng, e.Latitude, e.Longitude) <= radius)
                .ToList();
        }

        return Ok(result);
    }

    private static void SanitizeEvent(Event eventToSanitize)
    {
        eventToSanitize.Title = eventToSanitize.Title?.Trim() ?? string.Empty;
        eventToSanitize.Type = eventToSanitize.Type?.Trim() ?? string.Empty;
        eventToSanitize.Status = eventToSanitize.Status?.Trim() ?? string.Empty;
        eventToSanitize.Organizer = eventToSanitize.Organizer?.Trim() ?? string.Empty;
        eventToSanitize.Location = eventToSanitize.Location?.Trim() ?? string.Empty;
        eventToSanitize.Description = eventToSanitize.Description?.Trim() ?? string.Empty;
        eventToSanitize.CoverImageUrl = eventToSanitize.CoverImageUrl?.Trim() ?? string.Empty;
    }

    private static List<string> NormalizeImageUrls(IEnumerable<string>? imageUrls)
    {
        if (imageUrls == null)
        {
            return new List<string>();
        }

        return imageUrls
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Take(5)
            .Select(url => url.Trim())
            .ToList();
    }

    private static string ResolveCoverImage(string? coverImageUrl, IReadOnlyList<string> imageUrls)
    {
        if (!string.IsNullOrWhiteSpace(coverImageUrl))
        {
            return coverImageUrl.Trim();
        }

        return imageUrls.Any() ? imageUrls[0] : string.Empty;
    }

    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusKm = 6371.0;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        lat1 = DegreesToRadians(lat1);
        lat2 = DegreesToRadians(lat2);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
        => degrees * (Math.PI / 180.0);
}
