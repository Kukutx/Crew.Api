using Crew.Application.Events;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

internal sealed class GetFeedQueryHandler : IGetFeedQueryHandler
{
    private readonly AppDbContext _dbContext;
    private readonly GeometryFactory _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public GetFeedQueryHandler(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<GetFeedResult> HandleAsync(GetFeedQuery query, CancellationToken cancellationToken = default)
    {
        var origin = _geometryFactory.CreatePoint(new Coordinate(query.Longitude, query.Latitude));
        var radiusMeters = query.RadiusKm * 1000d;

        var normalizedTags = query.Tags?
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray() ?? Array.Empty<string>();

        // ✅ 用 IsWithinDistance(...) 代替不存在的 STDWithin(...)
        // ✅ useSpheroid: true 让距离与范围以米为单位（PostGIS 椭球计算）
        var baseQuery =
        from e in _dbContext.RoadTripEvents.AsNoTracking()
            // ❶ 命名参数 → 位置参数
        where EF.Functions.IsWithinDistance(e.Location, origin, radiusMeters, true)
        join m in _dbContext.EventMetrics.AsNoTracking() on e.Id equals m.EventId into metricGroup
        from metrics in metricGroup.DefaultIfEmpty()
            // ❷ 同理：命名参数 → 位置参数
        let distanceKm = EF.Functions.Distance(e.Location, origin, true) / 1000d
        // ❸ 空安全：避免 CS8602
        let registrations = metrics != null ? metrics.RegistrationsCount : 0
        let likes = metrics != null ? metrics.LikesCount : 0
        let engagement = (double)(registrations + likes)
        let lastModified = (metrics != null && metrics.UpdatedAt > e.CreatedAt) ? metrics.UpdatedAt : e.CreatedAt
        select new
        {
            Event = e,
            DistanceKm = distanceKm,
            Registrations = registrations,
            Likes = likes,
            Engagement = engagement,
            LastModified = lastModified
        };

        if (normalizedTags.Length > 0)
        {
            baseQuery = baseQuery.Where(x =>
                x.Event.Tags.Any(t => t.Tag != null && normalizedTags.Contains(t.Tag!.Name.ToUpperInvariant())));
        }

        if (FeedCursor.TryParse(query.Cursor, out var cursor))
        {
            const double distanceTolerance = 1e-6;
            const double engagementTolerance = 1e-6;

            baseQuery = baseQuery.Where(x =>
                x.Event.CreatedAt < cursor.CreatedAt ||
                (x.Event.CreatedAt == cursor.CreatedAt &&
                    (
                        x.DistanceKm > cursor.DistanceKm + distanceTolerance ||
                        (x.DistanceKm >= cursor.DistanceKm - distanceTolerance && x.DistanceKm <= cursor.DistanceKm + distanceTolerance &&
                            (
                                x.Engagement < cursor.Engagement - engagementTolerance ||
                                (x.Engagement >= cursor.Engagement - engagementTolerance && x.Engagement <= cursor.Engagement + engagementTolerance &&
                                    x.Event.Id.CompareTo(cursor.EventId) > 0)
                            ))
                    )));
        }

        var orderedQuery = baseQuery
            .OrderByDescending(x => x.Event.CreatedAt)
            // 可选：若要利用 GIST 最近邻优化，可把下面这一行换成 DistanceKnn（只用于排序）
            // .ThenBy(x => EF.Functions.DistanceKnn(x.Event.Location, origin))
            .ThenBy(x => x.DistanceKm)
            .ThenByDescending(x => x.Engagement)
            .ThenBy(x => x.Event.Id)
            .Select(x => new EventCard(
                x.Event.Id,
                x.Event.OwnerId,
                x.Event.Title,
                x.Event.Description,
                x.Event.StartTime,
                x.Event.CreatedAt,
                x.Event.Location.Y,
                x.Event.Location.X,
                x.DistanceKm,
                x.Registrations,
                x.Likes,
                x.Engagement,
                x.Event.Tags.Where(t => t.Tag != null).Select(t => t.Tag!.Name).ToList(),
                x.LastModified));

        var take = Math.Clamp(query.Limit, 1, 50) + 1;
        var items = await orderedQuery.Take(take).ToListAsync(cancellationToken);

        var hasMore = items.Count == take;
        var results = hasMore ? items.Take(query.Limit).ToList() : items;
        var nextCursor = hasMore ? FeedCursor.Encode(results[^1]) : null;

        return new GetFeedResult(results, nextCursor);
    }
}
