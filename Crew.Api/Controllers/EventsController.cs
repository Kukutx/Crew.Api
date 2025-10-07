using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Crew.Api.Data.DbContexts;
using Crew.Api.Entities;
using Crew.Api.Models;
using Crew.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private const int MaxParticipants = 5;
    private readonly AppDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public EventsController(AppDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventModal>>> GetAll(CancellationToken cancellationToken)
    {
        var entities = await _context.Events
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return Ok(entities.Select(MapToModal));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventModal>> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.Events.FindAsync(new object?[] { id }, cancellationToken);
        if (entity == null) return NotFound();
        return Ok(MapToModal(entity));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<EventModal>> Create(EventInputModel newEvent, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        SanitizeEvent(newEvent);

        var entity = new Event();
        ApplyDtoToEntity(newEvent, entity, isUpdate: false);

        entity.UserUid = currentUid;

        var creator = await _context.Users.FindAsync(new object?[] { currentUid }, cancellationToken);
        if (creator is not null && !string.Equals(creator.IdentityLabel, UserIdentityLabels.Organizer, StringComparison.Ordinal))
        {
            creator.IdentityLabel = UserIdentityLabels.Organizer;
        }

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
        await _context.SaveChangesAsync(cancellationToken);

        var dto = MapToModal(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, EventInputModel updatedEvent, CancellationToken cancellationToken)
    {
        var entity = await _context.Events.FindAsync(new object?[] { id }, cancellationToken);
        if (entity == null) return NotFound();

        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        if (!string.Equals(entity.UserUid, currentUid, StringComparison.Ordinal))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, AuthorizationPolicies.RequireAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        SanitizeEvent(updatedEvent);
        if (updatedEvent.StartTime.HasValue && updatedEvent.EndTime.HasValue &&
            updatedEvent.EndTime.Value < updatedEvent.StartTime.Value)
        {
            return BadRequest("End time cannot be earlier than the start time.");
        }

        ApplyDtoToEntity(updatedEvent, entity, isUpdate: true);
        entity.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/registrations")]
    [Authorize]
    public async Task<IActionResult> RegisterForEvent(int id, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        var @event = await _context.Events.FindAsync(new object?[] { id }, cancellationToken);
        if (@event is null)
        {
            return NotFound();
        }

        var existing = await _context.EventRegistrations.FindAsync(new object?[] { id, currentUid }, cancellationToken);
        if (existing is null)
        {
            if (@event.Participants >= MaxParticipants)
            {
                return BadRequest($"Event is full. Maximum participants: {MaxParticipants}.");
            }

            var now = DateTime.UtcNow;
            existing = new EventRegistration
            {
                EventId = id,
                UserUid = currentUid,
                RegisteredAt = now,
                Status = EventRegistrationStatuses.Pending,
                StatusUpdatedAt = now,
            };
            _context.EventRegistrations.Add(existing);
            @event.Participants = Math.Min(MaxParticipants, @event.Participants + 1);
        }

        var user = await _context.Users.FindAsync(new object?[] { currentUid }, cancellationToken);
        if (user is not null && !string.Equals(user.IdentityLabel, UserIdentityLabels.Organizer, StringComparison.Ordinal))
        {
            user.IdentityLabel = UserIdentityLabels.Participant;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/registrations/{userUid}")]
    [Authorize]
    public async Task<IActionResult> UpdateRegistration(int id, string userUid, UpdateEventRegistrationRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest("status is required.");
        }

        if (string.IsNullOrWhiteSpace(userUid))
        {
            return BadRequest("user id is required.");
        }

        var normalizedUserUid = userUid.Trim();

        var normalizedStatus = request.Status.Trim().ToLowerInvariant();
        if (!EventRegistrationStatuses.IsValid(normalizedStatus))
        {
            return BadRequest($"status must be one of: {string.Join(" / ", EventRegistrationStatuses.All)}.");
        }

        var @event = await _context.Events.FindAsync(new object?[] { id }, cancellationToken);
        if (@event is null)
        {
            return NotFound();
        }

        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        if (!string.Equals(@event.UserUid, currentUid, StringComparison.Ordinal))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, AuthorizationPolicies.RequireAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        var registration = await _context.EventRegistrations.FindAsync(new object?[] { id, normalizedUserUid }, cancellationToken);
        if (registration is null)
        {
            return NotFound();
        }

        if (!string.Equals(registration.Status, normalizedStatus, StringComparison.Ordinal))
        {
            registration.Status = normalizedStatus;
            registration.StatusUpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}/registrations/{userUid}")]
    [Authorize]
    public async Task<IActionResult> DeleteRegistration(int id, string userUid, CancellationToken cancellationToken)
    {
        var @event = await _context.Events.FindAsync(new object?[] { id }, cancellationToken);
        if (@event is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(userUid))
        {
            return BadRequest("user id is required.");
        }

        var normalizedUserUid = userUid.Trim();

        var registration = await _context.EventRegistrations.FindAsync(new object?[] { id, normalizedUserUid }, cancellationToken);
        if (registration is null)
        {
            return NoContent();
        }

        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        var isSelf = string.Equals(registration.UserUid, currentUid, StringComparison.Ordinal);
        var isOrganizer = string.Equals(@event.UserUid, currentUid, StringComparison.Ordinal);

        if (!isSelf && !isOrganizer)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, AuthorizationPolicies.RequireAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        _context.EventRegistrations.Remove(registration);
        if (@event.Participants > 0)
        {
            @event.Participants -= 1;
        }
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}/participants")]
    public async Task<ActionResult<IReadOnlyCollection<EventParticipantResponse>>> GetParticipants(int id, CancellationToken cancellationToken)
    {
        var participants = await _context.EventRegistrations
            .AsNoTracking()
            .Where(r => r.EventId == id && r.Status == EventRegistrationStatuses.Confirmed)
            .Join(
                _context.Users.AsNoTracking(),
                registration => registration.UserUid,
                user => user.Uid,
                (registration, user) => new EventParticipantResponse(
                    user.Uid,
                    string.IsNullOrWhiteSpace(user.DisplayName)
                        ? (string.IsNullOrWhiteSpace(user.UserName)
                            ? (string.IsNullOrWhiteSpace(user.Email) ? user.Uid : user.Email)
                            : user.UserName)
                        : user.DisplayName,
                    user.AvatarUrl,
                    registration.RegisteredAt,
                    registration.StatusUpdatedAt))
            .OrderByDescending(p => p.StatusUpdatedAt)
            .ToListAsync(cancellationToken);

        if (participants.Count == 0)
        {
            var eventExists = await _context.Events
                .AsNoTracking()
                .AnyAsync(e => e.Id == id, cancellationToken);

            if (!eventExists)
            {
                return NotFound();
            }
        }

        return Ok(participants);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.Events.FindAsync(new object?[] { id }, cancellationToken);
        if (entity == null) return NotFound();

        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        if (!string.Equals(entity.UserUid, currentUid, StringComparison.Ordinal))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, AuthorizationPolicies.RequireAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        _context.Events.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/favorite")]
    [Authorize]
    public async Task<IActionResult> FavoriteEvent(int id, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (currentUid is null)
        {
            return Forbid();
        }

        var eventExists = await _context.Events
            .AsNoTracking()
            .AnyAsync(e => e.Id == id, cancellationToken);
        if (!eventExists)
        {
            return NotFound();
        }

        var existing = await _context.EventFavorites.FindAsync(new object?[] { id, currentUid }, cancellationToken);
        if (existing != null)
        {
            return NoContent();
        }

        _context.EventFavorites.Add(new EventFavorite
        {
            EventId = id,
            UserUid = currentUid,
            CreatedAt = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}/favorite")]
    [Authorize]
    public async Task<IActionResult> UnfavoriteEvent(int id, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (currentUid is null)
        {
            return Forbid();
        }

        var favorite = await _context.EventFavorites.FindAsync(new object?[] { id, currentUid }, cancellationToken);
        if (favorite == null)
        {
            return NoContent();
        }

        _context.EventFavorites.Remove(favorite);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("user/{uid}")]
    public async Task<ActionResult<UserEventsResponse>> GetEventsForUser(string uid, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(uid))
        {
            return BadRequest("User UID is required.");
        }

        var normalizedUid = uid.Trim();

        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Uid == normalizedUid, cancellationToken);
        if (!userExists)
        {
            return NotFound();
        }

        var createdEvents = await _context.Events
            .AsNoTracking()
            .Where(e => e.UserUid == normalizedUid)
            .OrderByDescending(e => e.StartTime)
            .ToListAsync(cancellationToken);

        var favoritedEvents = await _context.EventFavorites
            .AsNoTracking()
            .Where(f => f.UserUid == normalizedUid)
            .Join(
                _context.Events.AsNoTracking(),
                favorite => favorite.EventId,
                evt => evt.Id,
                (favorite, evt) => evt)
            .OrderByDescending(e => e.StartTime)
            .ToListAsync(cancellationToken);

        var response = new UserEventsResponse(
            normalizedUid,
            createdEvents.Select(MapToModal).ToList(),
            favoritedEvents.Select(MapToModal).ToList());

        return Ok(response);
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

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = type.Trim().ToLower();
            events = events.Where(e =>
                e.Type != null && e.Type.ToLower() == normalizedType);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLower();
            events = events.Where(e =>
                e.Status != null && e.Status.ToLower() == normalizedStatus);
        }

        if (lat.HasValue && lng.HasValue && radiusKm.HasValue && radiusKm.Value > 0)
        {
            var originLat = lat.Value;
            var originLng = lng.Value;
            var radius = radiusKm.Value;

            var materialized = await events.ToListAsync();
            var result = materialized
                .Where(e => CalculateDistanceKm(originLat, originLng, e.Latitude, e.Longitude) <= radius)
                .ToList();

            return Ok(result.Select(MapToModal));
        }

        var projected = await events
            .Select(e => new EventModal
            {
                Id = e.Id,
                Title = e.Title,
                Type = e.Type,
                Status = e.Status,
                Location = e.Location,
                Description = e.Description,
                Participants = e.Participants,
                UserUid = e.UserUid,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                CreatedAt = e.CreatedAt,
                LastUpdated = e.LastUpdated,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                ImageUrls = e.ImageUrls,
                CoverImageUrl = e.CoverImageUrl
            })
            .ToListAsync();

        return Ok(projected);
    }

    private static void SanitizeEvent(EventInputModel eventToSanitize)
    {
        eventToSanitize.Title = eventToSanitize.Title?.Trim() ?? string.Empty;
        eventToSanitize.Type = eventToSanitize.Type?.Trim() ?? string.Empty;
        eventToSanitize.Status = eventToSanitize.Status?.Trim() ?? string.Empty;
        eventToSanitize.Location = eventToSanitize.Location?.Trim() ?? string.Empty;
        eventToSanitize.Description = eventToSanitize.Description?.Trim() ?? string.Empty;
        eventToSanitize.CoverImageUrl = eventToSanitize.CoverImageUrl?.Trim() ?? string.Empty;
        eventToSanitize.ImageUrls ??= new List<string>();
    }

    private static void ApplyDtoToEntity(EventInputModel source, Event target, bool isUpdate)
    {
        target.Title = source.Title;
        target.Type = source.Type;
        target.Status = source.Status;
        target.Location = source.Location;
        target.Description = source.Description;
        if (!isUpdate)
        {
            target.Participants = Math.Max(0, Math.Min(MaxParticipants, source.Participants));
        }

        if (isUpdate)
        {
            if (source.StartTime.HasValue)
            {
                target.StartTime = source.StartTime.Value;
            }

            if (source.EndTime.HasValue)
            {
                target.EndTime = source.EndTime.Value;
            }
        }
        else
        {
            if (source.StartTime.HasValue)
            {
                target.StartTime = source.StartTime.Value;
            }

            if (source.EndTime.HasValue)
            {
                target.EndTime = source.EndTime.Value;
            }
        }

        target.Latitude = source.Latitude;
        target.Longitude = source.Longitude;

        var normalizedImages = NormalizeImageUrls(source.ImageUrls);
        target.ImageUrls = normalizedImages;
        target.CoverImageUrl = ResolveCoverImage(source.CoverImageUrl, normalizedImages);
    }

    private static EventModal MapToModal(Event entity)
        => new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Type = entity.Type,
            Status = entity.Status,
            Location = entity.Location,
            Description = entity.Description,
            Participants = entity.Participants,
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
    private string? GetCurrentUserUid()
        => (User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier))?.Trim();
}

public record UpdateEventRegistrationRequest(string Status);

public record EventParticipantResponse(
    string Uid,
    string DisplayName,
    string AvatarUrl,
    DateTime RegisteredAt,
    DateTime StatusUpdatedAt);
