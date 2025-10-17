using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crew.Application.Auth;
using Crew.Contracts.Users;
using Crew.Domain.Enums;
using Crew.Tests.Support;

namespace Crew.Tests;

public class UsersControllerTests : IClassFixture<CrewApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly CrewApiFactory _factory;

    public UsersControllerTests(CrewApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task EnsureUser_CreatesProfileWithTagsAndBio()
    {
        await _factory.ResetDatabaseAsync();

        var client = _factory.CreateAuthenticatedClient();
        var request = new EnsureUserRequest(
            CrewApiFactory.TestFirebaseUid,
            "Integration Tester",
            "tester@example.com",
            nameof(UserRole.User),
            "https://example.com/avatar.jpg",
            "Road trip lover",
            new[] { "Camper", "Foodie" });

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users/ensure")
        {
            Content = CreateJsonContent(request)
        };

        var response = await client.SendAsync(httpRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadAsStringAsync();
        var profile = JsonSerializer.Deserialize<UserProfileDto>(payload, JsonOptions);
        profile.Should().NotBeNull();
        profile!.DisplayName.Should().Be("Integration Tester");
        profile.Email.Should().Be("tester@example.com");
        profile.Role.Should().Be("user");
        profile.Tags.Should().BeEquivalentTo(new[] { "Camper", "Foodie" });

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await dbContext.Users
            .Include(u => u.Tags)
                .ThenInclude(ut => ut.Tag)
            .SingleAsync();

        user.DisplayName.Should().Be("Integration Tester");
        user.Email.Should().Be("tester@example.com");
        user.AvatarUrl.Should().Be("https://example.com/avatar.jpg");
        user.Bio.Should().Be("Road trip lover");
        user.Tags.Select(t => t.Tag!.Name).Should().BeEquivalentTo(new[] { "Camper", "Foodie" });
    }

    [Fact]
    public async Task FollowAndUnfollow_ManageRelationships()
    {
        await _factory.ResetDatabaseAsync();

        Guid otherUserId;
        using (var scope = _factory.Services.CreateScope())
        {
            var provisioning = scope.ServiceProvider.GetRequiredService<UserProvisioningService>();
            var other = await provisioning.EnsureUserAsync("other-user", "Companion", cancellationToken: CancellationToken.None);
            otherUserId = other.Id;
        }

        var client = _factory.CreateAuthenticatedClient();

        var followResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"/api/v1/users/{otherUserId}/follow"));
        followResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var assertionScope = _factory.Services.CreateScope())
        {
            var dbContext = assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var currentUser = await dbContext.Users.SingleAsync(u => u.FirebaseUid == CrewApiFactory.TestFirebaseUid);

            var follow = await dbContext.UserFollows.SingleAsync();
            follow.FollowerId.Should().Be(currentUser.Id);
            follow.FollowingId.Should().Be(otherUserId);
        }

        var unfollowResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/users/{otherUserId}/follow"));
        unfollowResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var assertionScope = _factory.Services.CreateScope())
        {
            var dbContext = assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var remaining = await dbContext.UserFollows.ToListAsync();
            remaining.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task AddGuestbookEntry_PersistsFeedback()
    {
        await _factory.ResetDatabaseAsync();

        Guid ownerId;
        using (var scope = _factory.Services.CreateScope())
        {
            var provisioning = scope.ServiceProvider.GetRequiredService<UserProvisioningService>();
            var owner = await provisioning.EnsureUserAsync("owner-user", "Trip Host", cancellationToken: CancellationToken.None);
            ownerId = owner.Id;
        }

        var client = _factory.CreateAuthenticatedClient();
        var request = new AddGuestbookEntryRequest("Great experience!", 5);

        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"/api/v1/users/{ownerId}/guestbook")
        {
            Content = CreateJsonContent(request)
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadAsStringAsync();
        var entry = JsonSerializer.Deserialize<UserGuestbookEntryDto>(payload, JsonOptions);
        entry.Should().NotBeNull();
        entry!.Content.Should().Be("Great experience!");
        entry.Rating.Should().Be(5);
        entry.AuthorDisplayName.Should().Be("Integration Tester");

        using var assertionScope = _factory.Services.CreateScope();
        var dbContext = assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var stored = await dbContext.UserGuestbookEntries.SingleAsync();
        stored.OwnerId.Should().Be(ownerId);
        stored.Content.Should().Be("Great experience!");
        stored.Rating.Should().Be(5);
    }

    private static StringContent CreateJsonContent<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
