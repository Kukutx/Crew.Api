using Crew.Application.Events;
using Crew.Application.Moments;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using NetTopologySuite.Geometries;                    // EF.Functions
using Npgsql.EntityFrameworkCore.PostgreSQL;            // ILike/空间扩展注册
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite;

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
            query = query.Where(e => polygon.Contains(e.Location) ||
                         (e.EndPoint != null && polygon.Contains(e.EndPoint!)));
        }

        if (request.From is not null)
        {
            query = query.Where(e => e.StartTime >= request.From.Value);
        }

        if (request.To is not null)
        {
            query = query.Where(e => e.StartTime <= request.To.Value);
        }

        query = query
            .Include(e => e.Registrations)
            .Include(e => e.Tags)
                .ThenInclude(t => t.Tag);

        var summaries = await query
            .Select(e => new EventSummary(
                e.Id,
                e.OwnerId,
                e.Title,
                e.StartTime,
                e.Location.X,
                e.Location.Y,
                e.Registrations.Count(r => r.Status == Crew.Domain.Enums.RegistrationStatus.Confirmed),
                e.MaxParticipants,
                userId != null && e.Registrations.Any(r => r.UserId == userId),
                e.Tags
                    .Where(t => t.Tag != null)
                    .Select(t => t.Tag!.Name)
                    .ToList()))
            .ToListAsync(cancellationToken);

        return summaries;
    }

    public async Task<EventDetail?> GetDetailAsync(Guid eventId, Guid? userId, CancellationToken cancellationToken = default)
    {
        var @event = await _dbContext.RoadTripEvents
            .Include(e => e.Segments)
            .Include(e => e.Registrations)
            .Include(e => e.Tags)
                .ThenInclude(t => t.Tag)
            .Include(e => e.Moments)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (@event is null)
        {
            return null;
        }

        var segments = @event.Segments
            .OrderBy(s => s.Seq)
            .Select(s => new EventSegmentModel(s.Seq, s.Waypoint.X, s.Waypoint.Y, s.Note))
            .ToList();

        var tags = @event.Tags
            .Where(t => t.Tag != null)
            .Select(t => t.Tag!.Name)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var moments = @event.Moments
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MomentSummary(
                m.Id,
                m.UserId,
                m.User?.DisplayName,
                m.Title,
                m.CoverImageUrl,
                m.Country,
                m.City,
                m.CreatedAt))
            .ToList();

        var detail = new EventDetail(
            @event.Id,
            @event.OwnerId,
            @event.Title,
            @event.Description,
            @event.StartTime,
            @event.EndTime,
            @event.Location.X,
            @event.Location.Y,
            @event.EndPoint?.X,
            @event.EndPoint?.Y,
            @event.RoutePolyline,
            @event.MaxParticipants,
            @event.Visibility,
            segments,
            @event.Registrations.Count(r => r.Status == Crew.Domain.Enums.RegistrationStatus.Confirmed),
            userId != null && @event.Registrations.Any(r => r.UserId == userId),
            tags,
            moments);

        return detail;
    }
}
