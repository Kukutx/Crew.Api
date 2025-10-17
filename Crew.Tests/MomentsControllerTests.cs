using System.Linq;
using System.Text;
using Crew.Application.Auth;
using Crew.Contracts.Moments;
using Crew.Domain.Entities;
using Crew.Domain.Enums;
using Crew.Tests.Support;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Crew.Tests;

public class MomentsControllerTests : IClassFixture<CrewApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly CrewApiFactory _factory;

    public MomentsControllerTests(CrewApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateMoment_PersistsMomentWithImages()
    {
        await _factory.ResetDatabaseAsync();

        var eventId = await SeedEventAsync();

        var client = _factory.CreateAuthenticatedClient();
        var requestBody = new
        {
            eventId,
            title = "Sunset Adventure",
            content = "We had an incredible sunset drive!",
            coverImageUrl = "https://example.com/cover.jpg",
            country = "Taiwan",
            city = "Taipei",
            images = new[]
            {
                "https://example.com/1.jpg",
                "https://example.com/2.jpg",
                ""
            }
        };

        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/api/v1/moments")
        {
            Content = CreateJsonContent(requestBody)
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadAsStringAsync();
        var detail = JsonSerializer.Deserialize<MomentDetailDto>(payload, JsonOptions);
        detail.Should().NotBeNull();
        detail!.Title.Should().Be("Sunset Adventure");
        detail.Country.Should().Be("Taiwan");
        detail.Images.Should().ContainInOrder("https://example.com/1.jpg", "https://example.com/2.jpg");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var moment = await dbContext.Moments
            .Include(m => m.Images)
            .SingleAsync();

        moment.EventId.Should().Be(eventId);
        moment.Title.Should().Be("Sunset Adventure");
        moment.Images.Select(i => i.Url).Should().ContainInOrder("https://example.com/1.jpg", "https://example.com/2.jpg");
    }

    [Fact]
    public async Task AddComment_ReturnsCommentAndStoresIt()
    {
        await _factory.ResetDatabaseAsync();

        var client = _factory.CreateAuthenticatedClient();
        var detail = await CreateMomentAsync(client);

        var commentRequest = new { content = "Amazing views!" };

        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"/api/v1/moments/{detail.Id}/comments")
        {
            Content = CreateJsonContent(commentRequest)
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadAsStringAsync();
        var comment = JsonSerializer.Deserialize<MomentCommentDto>(payload, JsonOptions);
        comment.Should().NotBeNull();
        comment!.Content.Should().Be("Amazing views!");
        comment.AuthorDisplayName.Should().Be("Integration Tester");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var stored = await dbContext.MomentComments.SingleAsync();
        stored.MomentId.Should().Be(detail.Id);
        stored.Content.Should().Be("Amazing views!");
    }

    private async Task<Guid> SeedEventAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var provisioning = scope.ServiceProvider.GetRequiredService<UserProvisioningService>();
        var user = await provisioning.EnsureUserAsync(CrewApiFactory.TestFirebaseUid, "Integration Tester", cancellationToken: CancellationToken.None);

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var eventId = Guid.NewGuid();

        dbContext.RoadTripEvents.Add(new RoadTripEvent
        {
            Id = eventId,
            OwnerId = user.Id,
            Title = "Test Road Trip",
            Description = "Seeded event for moment tests",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(4),
            StartPoint = geometryFactory.CreatePoint(new Coordinate(25.03, 121.56)),
            Visibility = EventVisibility.Public,
            MaxParticipants = 7
        });

        await dbContext.SaveChangesAsync();
        return eventId;
    }

    private async Task<MomentDetailDto> CreateMomentAsync(HttpClient client)
    {
        var requestBody = new
        {
            eventId = (Guid?)null,
            title = "Coastal Drive",
            content = "The coast was clear and beautiful.",
            coverImageUrl = "https://example.com/moment-cover.jpg",
            country = "Japan",
            city = "Okinawa",
            images = new[] { "https://example.com/coast-1.jpg" }
        };

        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/api/v1/moments")
        {
            Content = CreateJsonContent(requestBody)
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MomentDetailDto>(payload, JsonOptions)!;
    }

    private static StringContent CreateJsonContent(object value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
