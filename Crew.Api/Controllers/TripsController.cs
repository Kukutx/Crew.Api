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
public class TripsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public TripsController(AppDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TripModal>>> GetAll(CancellationToken cancellationToken)
    {
        var trips = await _context.Trips
            .Include(t => t.Routes)
            .Include(t => t.Schedules)
            .AsNoTracking()
            .OrderByDescending(t => t.StartDate)
            .ToListAsync(cancellationToken);

        return Ok(trips.Select(MapToModal));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TripModal>> GetById(int id, CancellationToken cancellationToken)
    {
        var trip = await _context.Trips
            .Include(t => t.Routes)
            .Include(t => t.Schedules)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (trip is null)
        {
            return NotFound();
        }

        return Ok(MapToModal(trip));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TripModal>> Create(TripInputModel newTrip, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        SanitizeTrip(newTrip);

        var entity = new Trip
        {
            OrganizerUid = currentUid,
            CreatedAt = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow,
        };

        ApplyDtoToEntity(newTrip, entity, isUpdate: false);

        if (newTrip.Routes is { Count: > 0 })
        {
            entity.Routes = newTrip.Routes
                .OrderBy(r => r.OrderIndex)
                .Select(r => new TripRoute
                {
                    OrderIndex = r.OrderIndex,
                    Name = r.Name.Trim(),
                    Latitude = r.Latitude,
                    Longitude = r.Longitude,
                    Description = r.Description?.Trim() ?? string.Empty,
                })
                .ToList();
        }

        if (newTrip.Schedules is { Count: > 0 })
        {
            entity.Schedules = newTrip.Schedules
                .OrderBy(s => s.Date)
                .Select(s => new TripSchedule
                {
                    Date = s.Date,
                    Content = s.Content?.Trim() ?? string.Empty,
                    Hotel = s.Hotel?.Trim() ?? string.Empty,
                    Meal = s.Meal?.Trim() ?? string.Empty,
                    Note = s.Note?.Trim() ?? string.Empty,
                })
                .ToList();
        }

        _context.Trips.Add(entity);

        var creator = await _context.Users.FindAsync(new object?[] { currentUid }, cancellationToken);
        if (creator is not null && !string.Equals(creator.IdentityLabel, UserIdentityLabels.Organizer, StringComparison.Ordinal))
        {
            creator.IdentityLabel = UserIdentityLabels.Organizer;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = await _context.Trips
            .Include(t => t.Routes)
            .Include(t => t.Schedules)
            .AsNoTracking()
            .FirstAsync(t => t.Id == entity.Id, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, MapToModal(dto));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, TripInputModel updatedTrip, CancellationToken cancellationToken)
    {
        var trip = await _context.Trips
            .Include(t => t.Routes)
            .Include(t => t.Schedules)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (trip is null)
        {
            return NotFound();
        }

        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        if (!string.Equals(trip.OrganizerUid, currentUid, StringComparison.Ordinal))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, AuthorizationPolicies.RequireAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        SanitizeTrip(updatedTrip);

        if (updatedTrip.StartDate.HasValue && updatedTrip.EndDate.HasValue &&
            updatedTrip.EndDate.Value < updatedTrip.StartDate.Value)
        {
            return BadRequest("End date cannot be earlier than the start date.");
        }

        ApplyDtoToEntity(updatedTrip, trip, isUpdate: true);
        trip.LastUpdated = DateTime.UtcNow;

        if (updatedTrip.Routes is not null)
        {
            _context.TripRoutes.RemoveRange(trip.Routes);
            trip.Routes.Clear();

            foreach (var route in updatedTrip.Routes.OrderBy(r => r.OrderIndex))
            {
                trip.Routes.Add(new TripRoute
                {
                    OrderIndex = route.OrderIndex,
                    Name = route.Name.Trim(),
                    Latitude = route.Latitude,
                    Longitude = route.Longitude,
                    Description = route.Description?.Trim() ?? string.Empty,
                });
            }
        }

        if (updatedTrip.Schedules is not null)
        {
            _context.TripSchedules.RemoveRange(trip.Schedules);
            trip.Schedules.Clear();

            foreach (var schedule in updatedTrip.Schedules.OrderBy(s => s.Date))
            {
                trip.Schedules.Add(new TripSchedule
                {
                    Date = schedule.Date,
                    Content = schedule.Content?.Trim() ?? string.Empty,
                    Hotel = schedule.Hotel?.Trim() ?? string.Empty,
                    Meal = schedule.Meal?.Trim() ?? string.Empty,
                    Note = schedule.Note?.Trim() ?? string.Empty,
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/participants")]
    [Authorize]
    public async Task<IActionResult> JoinTrip(int id, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        var trip = await _context.Trips.FindAsync(new object?[] { id }, cancellationToken);
        if (trip is null)
        {
            return NotFound();
        }

        var existing = await _context.TripParticipants.FindAsync(new object?[] { id, currentUid }, cancellationToken);
        if (existing is null)
        {
            existing = new TripParticipant
            {
                TripId = id,
                UserUid = currentUid,
                JoinTime = DateTime.UtcNow,
                Status = TripParticipantStatuses.Pending,
                Role = TripParticipantRoles.Passenger,
            };
            _context.TripParticipants.Add(existing);
        }

        var user = await _context.Users.FindAsync(new object?[] { currentUid }, cancellationToken);
        if (user is not null && !string.Equals(user.IdentityLabel, UserIdentityLabels.Organizer, StringComparison.Ordinal))
        {
            user.IdentityLabel = UserIdentityLabels.Participant;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/participants/{userUid}")]
    [Authorize]
    public async Task<IActionResult> UpdateParticipant(int id, string userUid, UpdateTripParticipantRequest request, CancellationToken cancellationToken)
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
        var normalizedStatus = request.Status.Trim();
        if (!TripParticipantStatuses.IsValid(normalizedStatus))
        {
            return BadRequest($"status must be one of: {string.Join(" / ", TripParticipantStatuses.All)}.");
        }

        var trip = await _context.Trips.FindAsync(new object?[] { id }, cancellationToken);
        if (trip is null)
        {
            return NotFound();
        }

        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        if (!string.Equals(trip.OrganizerUid, currentUid, StringComparison.Ordinal))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, AuthorizationPolicies.RequireAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        var participant = await _context.TripParticipants.FindAsync(new object?[] { id, normalizedUserUid }, cancellationToken);
        if (participant is null)
        {
            return NotFound();
        }

        participant.Status = normalizedStatus.ToLowerInvariant();
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}/participants/{userUid}")]
    [Authorize]
    public async Task<IActionResult> RemoveParticipant(int id, string userUid, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userUid))
        {
            return BadRequest("user id is required.");
        }

        var trip = await _context.Trips.FindAsync(new object?[] { id }, cancellationToken);
        if (trip is null)
        {
            return NotFound();
        }

        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        if (!string.Equals(trip.OrganizerUid, currentUid, StringComparison.Ordinal))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, AuthorizationPolicies.RequireAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        var participant = await _context.TripParticipants.FindAsync(new object?[] { id, userUid.Trim() }, cancellationToken);
        if (participant is null)
        {
            return NotFound();
        }

        _context.TripParticipants.Remove(participant);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}/participants")]
    public async Task<ActionResult<IReadOnlyCollection<TripParticipantResponse>>> GetParticipants(int id, CancellationToken cancellationToken)
    {
        var participants = await _context.TripParticipants
            .Where(p => p.TripId == id && p.Status == TripParticipantStatuses.Confirmed)
            .Join(
                _context.Users.AsNoTracking(),
                participant => participant.UserUid,
                user => user.Uid,
                (participant, user) => new TripParticipantResponse(
                    participant.UserUid,
                    user.DisplayName,
                    user.AvatarUrl,
                    participant.Role,
                    participant.Status,
                    participant.JoinTime))
            .ToListAsync(cancellationToken);

        return Ok(participants);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var trip = await _context.Trips.FindAsync(new object?[] { id }, cancellationToken);
        if (trip is null)
        {
            return NotFound();
        }

        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        if (!string.Equals(trip.OrganizerUid, currentUid, StringComparison.Ordinal))
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, AuthorizationPolicies.RequireAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
        }

        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/favorite")]
    [Authorize]
    public async Task<IActionResult> FavoriteTrip(int id, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        var tripExists = await _context.Trips
            .AsNoTracking()
            .AnyAsync(t => t.Id == id, cancellationToken);

        if (!tripExists)
        {
            return NotFound();
        }

        var existing = await _context.TripFavorites.FindAsync(new object?[] { id, currentUid }, cancellationToken);
        if (existing is null)
        {
            _context.TripFavorites.Add(new TripFavorite
            {
                TripId = id,
                UserUid = currentUid,
                CreatedAt = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }

    [HttpDelete("{id}/favorite")]
    [Authorize]
    public async Task<IActionResult> UnfavoriteTrip(int id, CancellationToken cancellationToken)
    {
        var currentUid = GetCurrentUserUid();
        if (string.IsNullOrWhiteSpace(currentUid))
        {
            return Forbid();
        }

        var favorite = await _context.TripFavorites.FindAsync(new object?[] { id, currentUid }, cancellationToken);
        if (favorite is null)
        {
            return NotFound();
        }

        _context.TripFavorites.Remove(favorite);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("by-user/{uid}")]
    public async Task<ActionResult<UserTripsResponse>> GetTripsForUser(string uid, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(uid))
        {
            return BadRequest("uid is required");
        }

        var normalizedUid = uid.Trim();

        var createdTrips = await _context.Trips
            .Include(t => t.Routes)
            .Include(t => t.Schedules)
            .AsNoTracking()
            .Where(t => t.OrganizerUid == normalizedUid)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync(cancellationToken);

        var favoritedTrips = await _context.TripFavorites
            .Where(f => f.UserUid == normalizedUid)
            .Join(
                _context.Trips
                    .Include(t => t.Routes)
                    .Include(t => t.Schedules)
                    .AsNoTracking(),
                favorite => favorite.TripId,
                trip => trip.Id,
                (_, trip) => trip)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync(cancellationToken);

        var response = new UserTripsResponse(
            createdTrips.Select(MapToModal).ToList(),
            favoritedTrips.Select(MapToModal).ToList());

        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TripModal>>> SearchTrips(
        [FromQuery(Name = "q")] string? query,
        [FromQuery] string? status,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end,
        CancellationToken cancellationToken)
    {
        var trips = _context.Trips
            .Include(t => t.Routes)
            .Include(t => t.Schedules)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.Trim();
            trips = trips.Where(t =>
                EF.Functions.Like(t.Title, $"%{normalizedQuery}%") ||
                EF.Functions.Like(t.Description, $"%{normalizedQuery}%") ||
                EF.Functions.Like(t.StartLocation, $"%{normalizedQuery}%") ||
                EF.Functions.Like(t.EndLocation, $"%{normalizedQuery}%"));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim();
            if (!TripStatuses.IsValid(normalizedStatus))
            {
                return BadRequest($"status must be one of: {string.Join(" / ", TripStatuses.All)}.");
            }

            trips = trips.Where(t => t.Status == normalizedStatus);
        }

        if (start.HasValue)
        {
            trips = trips.Where(t => t.StartDate >= start.Value);
        }

        if (end.HasValue)
        {
            trips = trips.Where(t => t.EndDate <= end.Value);
        }

        var results = await trips
            .AsNoTracking()
            .OrderByDescending(t => t.StartDate)
            .ToListAsync(cancellationToken);

        return Ok(results.Select(MapToModal));
    }

    private static void SanitizeTrip(TripInputModel tripToSanitize)
    {
        if (tripToSanitize is null)
        {
            throw new ArgumentNullException(nameof(tripToSanitize));
        }

        if (tripToSanitize.Title is not null)
        {
            tripToSanitize.Title = tripToSanitize.Title.Trim();
        }

        if (tripToSanitize.Description is not null)
        {
            tripToSanitize.Description = tripToSanitize.Description.Trim();
        }

        if (tripToSanitize.StartLocation is not null)
        {
            tripToSanitize.StartLocation = tripToSanitize.StartLocation.Trim();
        }

        if (tripToSanitize.EndLocation is not null)
        {
            tripToSanitize.EndLocation = tripToSanitize.EndLocation.Trim();
        }

        if (!string.IsNullOrWhiteSpace(tripToSanitize.Status) && !TripStatuses.IsValid(tripToSanitize.Status))
        {
            tripToSanitize.Status = TripStatuses.Planning;
        }

        if (tripToSanitize.ExpectedParticipants.HasValue && tripToSanitize.ExpectedParticipants < 0)
        {
            tripToSanitize.ExpectedParticipants = 0;
        }

        if (tripToSanitize.Routes is not null)
        {
            tripToSanitize.Routes = tripToSanitize.Routes
                .Select(r => r with
                {
                    Name = r.Name.Trim(),
                    Description = r.Description?.Trim() ?? string.Empty,
                })
                .ToList();
        }

        if (tripToSanitize.Schedules is not null)
        {
            tripToSanitize.Schedules = tripToSanitize.Schedules
                .Select(s => s with
                {
                    Content = s.Content?.Trim(),
                    Hotel = s.Hotel?.Trim(),
                    Meal = s.Meal?.Trim(),
                    Note = s.Note?.Trim(),
                })
                .ToList();
        }
    }

    private static void ApplyDtoToEntity(TripInputModel source, Trip target, bool isUpdate)
    {
        if (!string.IsNullOrWhiteSpace(source.Title))
        {
            target.Title = source.Title.Trim();
        }
        else if (!isUpdate)
        {
            target.Title = "未命名行程";
        }

        if (!string.IsNullOrWhiteSpace(source.Status))
        {
            target.Status = source.Status.Trim();
        }
        else if (!isUpdate)
        {
            target.Status = TripStatuses.Planning;
        }

        if (!string.IsNullOrWhiteSpace(source.Description))
        {
            target.Description = source.Description.Trim();
        }

        if (source.StartDate.HasValue)
        {
            target.StartDate = source.StartDate.Value;
        }
        else if (!isUpdate)
        {
            target.StartDate = DateTime.UtcNow;
        }

        if (source.EndDate.HasValue)
        {
            target.EndDate = source.EndDate.Value;
        }
        else if (!isUpdate)
        {
            target.EndDate = target.StartDate;
        }

        if (!string.IsNullOrWhiteSpace(source.StartLocation))
        {
            target.StartLocation = source.StartLocation.Trim();
        }

        if (!string.IsNullOrWhiteSpace(source.EndLocation))
        {
            target.EndLocation = source.EndLocation.Trim();
        }

        if (source.ExpectedParticipants.HasValue)
        {
            target.ExpectedParticipants = Math.Max(0, source.ExpectedParticipants.Value);
        }

        if (source.StartLatitude.HasValue)
        {
            target.StartLatitude = source.StartLatitude;
        }

        if (source.StartLongitude.HasValue)
        {
            target.StartLongitude = source.StartLongitude;
        }

        if (source.EndLatitude.HasValue)
        {
            target.EndLatitude = source.EndLatitude;
        }

        if (source.EndLongitude.HasValue)
        {
            target.EndLongitude = source.EndLongitude;
        }

        if (!string.IsNullOrWhiteSpace(source.CoverImageUrl))
        {
            target.CoverImageUrl = source.CoverImageUrl.Trim();
        }
    }

    private static TripModal MapToModal(Trip trip)
        => new()
        {
            Id = trip.Id,
            Title = trip.Title,
            Status = trip.Status,
            Description = trip.Description,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            StartLocation = trip.StartLocation,
            EndLocation = trip.EndLocation,
            ExpectedParticipants = trip.ExpectedParticipants,
            StartLatitude = trip.StartLatitude,
            StartLongitude = trip.StartLongitude,
            EndLatitude = trip.EndLatitude,
            EndLongitude = trip.EndLongitude,
            CoverImageUrl = trip.CoverImageUrl,
            OrganizerUid = trip.OrganizerUid,
            CreatedAt = trip.CreatedAt,
            LastUpdated = trip.LastUpdated,
            Routes = trip.Routes.OrderBy(r => r.OrderIndex).ToList(),
            Schedules = trip.Schedules.OrderBy(s => s.Date).ToList(),
        };

    private string? GetCurrentUserUid()
        => User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
}

public record UpdateTripParticipantRequest(string Status);

public record TripParticipantResponse(
    string UserUid,
    string DisplayName,
    string AvatarUrl,
    string Role,
    string Status,
    DateTime JoinTime);
