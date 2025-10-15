using Crew.Application.Events;
using System.Linq;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Crew.Infrastructure.Services;

internal sealed class EventReadService : IEventReadService
{
    private readonly AppDbContext _dbContext;
    private readonly GeometryFactory _geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public EventReadService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EventSummary>> SearchAsync(EventSearchRequest request, Guid? userId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.RoadTripEvents.AsQueryable();

        if (request.MinLongitude is not null && request.MinLatitude is not null && request.MaxLongitude is not null && request.MaxLatitude is not null)
        {
            var envelope = new Envelope(request.MinLongitude.Value, request.MaxLongitude.Value, request.MinLatitude.Value, request.MaxLatitude.Value);
            var polygon = _geometryFactory.ToGeometry(envelope);
            query = query.Where(e => EF.Functions.Contains(polygon, e.StartPoint) || (e.EndPoint != null && EF.Functions.Contains(polygon, e.EndPoint!)));
        }

        if (request.From is not null)
        {
            query = query.Where(e => e.StartTime >= request.From.Value);
        }

        if (request.To is not null)
        {
            query = query.Where(e => e.StartTime <= request.To.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var pattern = $"%{request.Query.Trim()}%";
            query = query.Where(e =>
                EF.Functions.Like(EF.Functions.Collate(e.Title, "NOCASE"), pattern) ||
                (e.Description != null && EF.Functions.Like(EF.Functions.Collate(e.Description, "NOCASE"), pattern)));
        }

        query = query.Include(e => e.Registrations);

        var summaries = await query
            .Select(e => new EventSummary(
                e.Id,
                e.Title,
                e.StartTime,
                e.StartPoint.X,
                e.StartPoint.Y,
                e.Registrations.Count(r => r.Status == Crew.Domain.Enums.RegistrationStatus.Confirmed),
                userId != null && e.Registrations.Any(r => r.UserId == userId)))
            .ToListAsync(cancellationToken);

        return summaries;
    }

    public async Task<EventDetail?> GetDetailAsync(Guid eventId, Guid? userId, CancellationToken cancellationToken = default)
    {
        var @event = await _dbContext.RoadTripEvents
            .Include(e => e.Segments)
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (@event is null)
        {
            return null;
        }

        var segments = @event.Segments
            .OrderBy(s => s.Seq)
            .Select(s => new EventSegmentModel(s.Seq, s.Waypoint.X, s.Waypoint.Y, s.Note))
            .ToList();

        var detail = new EventDetail(
            @event.Id,
            @event.OwnerId,
            @event.Title,
            @event.Description,
            @event.StartTime,
            @event.EndTime,
            @event.StartPoint.X,
            @event.StartPoint.Y,
            @event.EndPoint?.X,
            @event.EndPoint?.Y,
            @event.RoutePolyline,
            @event.MaxParticipants,
            @event.Visibility,
            segments,
            @event.Registrations.Count(r => r.Status == Crew.Domain.Enums.RegistrationStatus.Confirmed),
            userId != null && @event.Registrations.Any(r => r.UserId == userId));

        return detail;
    }
}
