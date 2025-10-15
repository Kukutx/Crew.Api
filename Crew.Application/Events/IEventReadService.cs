namespace Crew.Application.Events;

public interface IEventReadService
{
    Task<IReadOnlyList<EventSummary>> SearchAsync(EventSearchRequest request, Guid? userId, CancellationToken cancellationToken = default);
    Task<EventDetail?> GetDetailAsync(Guid eventId, Guid? userId, CancellationToken cancellationToken = default);
}

public sealed record EventSearchRequest(
    double? MinLongitude,
    double? MinLatitude,
    double? MaxLongitude,
    double? MaxLatitude,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Query);
