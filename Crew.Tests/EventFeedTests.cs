using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Crew.Contracts.Events;
using Crew.Domain.Entities;
using Crew.Domain.Enums;
using Crew.Infrastructure.Persistence;
using Crew.Tests.Support;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Crew.Tests;

public class EventFeedTests : IClassFixture<CrewApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly CrewApiFactory _factory;

    public EventFeedTests(CrewApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetFeed_ReturnsOrderedEventsAndHonorsETag()
    {
        await _factory.ResetDatabaseAsync();

        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var now = DateTimeOffset.UtcNow;

        var latestEventId = Guid.NewGuid();
        var earlierEventId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var latestPoint = geometryFactory.CreatePoint(new Coordinate(121.56, 25.03));
            var earlierPoint = geometryFactory.CreatePoint(new Coordinate(121.50, 25.01));

            dbContext.RoadTripEvents.AddRange(
                new RoadTripEvent
                {
                    Id = latestEventId,
                    OwnerId = Guid.NewGuid(),
                    Title = "Mountain Cruise",
                    Description = "Latest adventure",
                    StartTime = now.AddDays(1),
                    CreatedAt = now,
                    StartPoint = latestPoint,
                    Location = latestPoint,
                    Visibility = EventVisibility.Public
                },
                new RoadTripEvent
                {
                    Id = earlierEventId,
                    OwnerId = Guid.NewGuid(),
                    Title = "City Night Ride",
                    Description = "Earlier trip",
                    StartTime = now.AddHours(12),
                    CreatedAt = now.AddHours(-2),
                    StartPoint = earlierPoint,
                    Location = earlierPoint,
                    Visibility = EventVisibility.Public
                });

            dbContext.EventMetrics.AddRange(
                new EventMetrics
                {
                    EventId = latestEventId,
                    LikesCount = 5,
                    RegistrationsCount = 3,
                    UpdatedAt = now
                },
                new EventMetrics
                {
                    EventId = earlierEventId,
                    LikesCount = 2,
                    RegistrationsCount = 1,
                    UpdatedAt = now
                });

            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var requestUri = "/api/v1/events/feed?lat=25.03&lng=121.56&radius=150&limit=1";

        var firstResponse = await client.GetAsync(requestUri);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstResponse.Headers.ETag.Should().NotBeNull();

        var body = await firstResponse.Content.ReadAsStringAsync();
        var feed = JsonSerializer.Deserialize<EventFeedResponseDto>(body, JsonOptions);
        feed.Should().NotBeNull();
        feed!.Events.Should().HaveCount(1);
        feed.Events[0].Id.Should().Be(latestEventId);
        feed.NextCursor.Should().NotBeNullOrEmpty();

        var etag = firstResponse.Headers.ETag!.Tag;
        var secondRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
        secondRequest.Headers.TryAddWithoutValidation("If-None-Match", etag);

        var secondResponse = await client.SendAsync(secondRequest);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task GetFeed_ChangesEtagWhenMetricsMutate()
    {
        await _factory.ResetDatabaseAsync();

        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var now = DateTimeOffset.UtcNow;
        var eventId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var point = geometryFactory.CreatePoint(new Coordinate(121.56, 25.03));

            dbContext.RoadTripEvents.Add(new RoadTripEvent
            {
                Id = eventId,
                OwnerId = Guid.NewGuid(),
                Title = "Metric Test",
                Description = "Initial",
                StartTime = now.AddDays(1),
                CreatedAt = now,
                StartPoint = point,
                Location = point,
                Visibility = EventVisibility.Public
            });

            dbContext.EventMetrics.Add(new EventMetrics
            {
                EventId = eventId,
                LikesCount = 0,
                RegistrationsCount = 0,
                UpdatedAt = now
            });

            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var requestUri = "/api/v1/events/feed?lat=25.03&lng=121.56&radius=150&limit=1";

        var firstResponse = await client.GetAsync(requestUri);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstEtag = firstResponse.Headers.ETag!.Tag;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var metrics = await dbContext.EventMetrics.SingleAsync();
            metrics.LikesCount = 5;
            metrics.UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(1);
            await dbContext.SaveChangesAsync();
        }

        var secondRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
        secondRequest.Headers.TryAddWithoutValidation("If-None-Match", firstEtag);

        var secondResponse = await client.SendAsync(secondRequest);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.Headers.ETag.Should().NotBeNull();
        secondResponse.Headers.ETag!.Tag.Should().NotBe(firstEtag);
    }

    [Fact]
    public async Task GetFeed_PaginatesWithCompositeOrdering()
    {
        await _factory.ResetDatabaseAsync();

        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var now = DateTimeOffset.UtcNow;

        var nearPoint = geometryFactory.CreatePoint(new Coordinate(121.5600, 25.0300));
        var midPoint = geometryFactory.CreatePoint(new Coordinate(121.5610, 25.0310));
        var farPoint = geometryFactory.CreatePoint(new Coordinate(121.5700, 25.0400));

        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var thirdId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.RoadTripEvents.AddRange(
                new RoadTripEvent
                {
                    Id = firstId,
                    OwnerId = Guid.NewGuid(),
                    Title = "Closest",
                    Description = "First",
                    StartTime = now.AddHours(1),
                    CreatedAt = now,
                    StartPoint = nearPoint,
                    Location = nearPoint,
                    Visibility = EventVisibility.Public
                },
                new RoadTripEvent
                {
                    Id = secondId,
                    OwnerId = Guid.NewGuid(),
                    Title = "Middle",
                    Description = "Second",
                    StartTime = now.AddHours(2),
                    CreatedAt = now,
                    StartPoint = midPoint,
                    Location = midPoint,
                    Visibility = EventVisibility.Public
                },
                new RoadTripEvent
                {
                    Id = thirdId,
                    OwnerId = Guid.NewGuid(),
                    Title = "Farthest",
                    Description = "Third",
                    StartTime = now.AddHours(3),
                    CreatedAt = now,
                    StartPoint = farPoint,
                    Location = farPoint,
                    Visibility = EventVisibility.Public
                });

            dbContext.EventMetrics.AddRange(
                new EventMetrics { EventId = firstId, LikesCount = 2, RegistrationsCount = 1, UpdatedAt = now },
                new EventMetrics { EventId = secondId, LikesCount = 1, RegistrationsCount = 1, UpdatedAt = now },
                new EventMetrics { EventId = thirdId, LikesCount = 0, RegistrationsCount = 0, UpdatedAt = now });

            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var firstPageResponse = await client.GetAsync("/api/v1/events/feed?lat=25.03&lng=121.56&radius=150&limit=2");
        firstPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstPayload = JsonSerializer.Deserialize<EventFeedResponseDto>(await firstPageResponse.Content.ReadAsStringAsync(), JsonOptions);
        firstPayload.Should().NotBeNull();
        firstPayload!.Events.Should().HaveCount(2);
        firstPayload.Events[0].Id.Should().Be(firstId);
        firstPayload.Events[1].Id.Should().Be(secondId);
        firstPayload.NextCursor.Should().NotBeNull();

        var encodedCursor = Uri.EscapeDataString(firstPayload.NextCursor!);
        var secondPageResponse = await client.GetAsync($"/api/v1/events/feed?lat=25.03&lng=121.56&radius=150&limit=2&cursor={encodedCursor}");
        secondPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondPayload = JsonSerializer.Deserialize<EventFeedResponseDto>(await secondPageResponse.Content.ReadAsStringAsync(), JsonOptions);
        secondPayload.Should().NotBeNull();
        secondPayload!.Events.Should().ContainSingle();
        secondPayload.Events[0].Id.Should().Be(thirdId);
    }
}
