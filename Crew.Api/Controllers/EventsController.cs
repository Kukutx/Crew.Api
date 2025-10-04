using System;
using System.Collections.Generic;
using System.Linq;
using Crew.Api.Data.DbContexts;
using Crew.Api.Entities;
using Crew.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EventsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventModal>>> GetAll()
    {
        var entities = await _context.Events.ToListAsync();
        return Ok(entities.Select(MapToModal));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventModal>> GetById(int id)
    {
        var entity = await _context.Events.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(MapToModal(entity));
    }

    [HttpPost]
    public async Task<ActionResult<EventModal>> Create(EventModal newEvent)
    {
        SanitizeEvent(newEvent);

        if (string.IsNullOrWhiteSpace(newEvent.UserUid))
        {
            return BadRequest("User UID is required.");
        }

        var entity = new Event();
        ApplyDtoToEntity(newEvent, entity, isUpdate: false);

        entity.Id = _context.Events.Any() ? _context.Events.Max(e => e.Id) + 1 : 1;

        if (entity.StartTime == default)
        {
            entity.StartTime = DateTime.UtcNow;
        }

        if (entity.EndTime == default)
        {
            entity.EndTime = entity.StartTime;
        }

        if (entity.EndTime < entity.StartTime)
        {
            return BadRequest("End time cannot be earlier than the start time.");
        }

        entity.CreatedAt = DateTime.UtcNow;
        entity.LastUpdated = entity.CreatedAt;

        _context.Events.Add(entity);
        await _context.SaveChangesAsync();

        var dto = MapToModal(entity);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, EventModal updatedEvent)
    {
        var entity = await _context.Events.FindAsync(id);
        if (entity == null) return NotFound();

        SanitizeEvent(updatedEvent);
        if (updatedEvent.StartTime != default && updatedEvent.EndTime != default &&
            updatedEvent.EndTime < updatedEvent.StartTime)
        {
            return BadRequest("End time cannot be earlier than the start time.");
        }

        ApplyDtoToEntity(updatedEvent, entity, isUpdate: true);
        entity.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Events.FindAsync(id);
        if (entity == null) return NotFound();

        _context.Events.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<EventModal>>> SearchEvents(
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

        return Ok(result.Select(MapToModal));
    }

    private static void SanitizeEvent(EventModal eventToSanitize)
    {
        eventToSanitize.Title = eventToSanitize.Title?.Trim() ?? string.Empty;
        eventToSanitize.Type = eventToSanitize.Type?.Trim() ?? string.Empty;
        eventToSanitize.Status = eventToSanitize.Status?.Trim() ?? string.Empty;
        eventToSanitize.Organizer = eventToSanitize.Organizer?.Trim() ?? string.Empty;
        eventToSanitize.Location = eventToSanitize.Location?.Trim() ?? string.Empty;
        eventToSanitize.Description = eventToSanitize.Description?.Trim() ?? string.Empty;
        eventToSanitize.CoverImageUrl = eventToSanitize.CoverImageUrl?.Trim() ?? string.Empty;
        eventToSanitize.UserUid = eventToSanitize.UserUid?.Trim() ?? string.Empty;
    }

    private static void ApplyDtoToEntity(EventModal source, Event target, bool isUpdate)
    {
        target.Title = source.Title;
        target.Type = source.Type;
        target.Status = source.Status;
        target.Organizer = source.Organizer;
        target.Location = source.Location;
        target.Description = source.Description;
        target.ExpectedParticipants = Math.Max(0, source.ExpectedParticipants);

        if (isUpdate)
        {
            if (source.StartTime != default)
            {
                target.StartTime = source.StartTime;
            }

            if (source.EndTime != default)
            {
                target.EndTime = source.EndTime;
            }
        }
        else
        {
            target.StartTime = source.StartTime;
            target.EndTime = source.EndTime;
        }

        target.Latitude = source.Latitude;
        target.Longitude = source.Longitude;

        var normalizedImages = NormalizeImageUrls(source.ImageUrls);
        target.ImageUrls = normalizedImages;
        target.CoverImageUrl = ResolveCoverImage(source.CoverImageUrl, normalizedImages);

        if (!string.IsNullOrEmpty(source.UserUid))
        {
            target.UserUid = source.UserUid;
        }
    }

    private static EventModal MapToModal(Event entity)
        => new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Type = entity.Type,
            Status = entity.Status,
            Organizer = entity.Organizer,
            Location = entity.Location,
            Description = entity.Description,
            ExpectedParticipants = entity.ExpectedParticipants,
            UserUid = entity.UserUid,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            CreatedAt = entity.CreatedAt,
            LastUpdated = entity.LastUpdated,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            ImageUrls = entity.ImageUrls?.ToList() ?? new List<string>(),
            CoverImageUrl = entity.CoverImageUrl
        };

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
