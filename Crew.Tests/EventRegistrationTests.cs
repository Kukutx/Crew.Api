using Crew.Domain.Entities;
using Crew.Domain.Enums;
using Crew.Domain.Events;
using Crew.Infrastructure.Persistence;
using Crew.Tests.Support;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;

namespace Crew.Tests;

public class EventRegistrationTests : IClassFixture<CrewApiFactory>
{
    private readonly CrewApiFactory _factory;

    public EventRegistrationTests(CrewApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterForEvent_CreatesRegistrationAndChatMembership()
    {
        await _factory.ResetDatabaseAsync();

        var eventId = Guid.NewGuid();
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var start = geometryFactory.CreatePoint(new Coordinate(30.0, 50.0));

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.RoadTripEvents.Add(new RoadTripEvent
            {
                Id = eventId,
                OwnerId = Guid.NewGuid(),
                Title = "Integration Ride",
                Description = "Testing registration",
                StartTime = DateTimeOffset.UtcNow,
                EndTime = DateTimeOffset.UtcNow.AddHours(2),
                StartPoint = start,
                Location = start,
                CreatedAt = DateTimeOffset.UtcNow,
                Visibility = EventVisibility.Public
            });

            await dbContext.SaveChangesAsync();
        }

        var response = await _factory.CreateAuthenticatedClient()
            .SendAsync(new HttpRequestMessage(HttpMethod.Post, $"/api/v1/events/{eventId}/registrations"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var assertionScope = _factory.Services.CreateScope();
        var assertDbContext = assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await assertDbContext.Users.SingleAsync(u => u.FirebaseUid == CrewApiFactory.TestFirebaseUid);
        var registration = await assertDbContext.Registrations.SingleAsync();
        registration.EventId.Should().Be(eventId);
        registration.UserId.Should().Be(user.Id);
        registration.Status.Should().Be(RegistrationStatus.Confirmed);

        var chat = await assertDbContext.Chats.SingleAsync();
        chat.EventId.Should().Be(eventId);
        chat.Type.Should().Be(ChatType.EventGroup);

        var membership = await assertDbContext.ChatMembers.SingleAsync();
        membership.ChatId.Should().Be(chat.Id);
        membership.UserId.Should().Be(user.Id);
        membership.Role.Should().Be(ChatMemberRole.Member);

        var outboxMessage = await assertDbContext.OutboxMessages.SingleAsync();
        outboxMessage.Type.Should().Be(nameof(UserJoinedGroupEvent));

        var history = await assertDbContext.UserActivityHistories.SingleAsync();
        history.UserId.Should().Be(user.Id);
        history.EventId.Should().Be(eventId);
        history.Role.Should().Be(ActivityRole.Participant);

        var metrics = await assertDbContext.EventMetrics.SingleAsync();
        metrics.EventId.Should().Be(eventId);
        metrics.RegistrationsCount.Should().Be(1);
        metrics.LikesCount.Should().Be(0);
    }
}
