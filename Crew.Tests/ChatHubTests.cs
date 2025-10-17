using Crew.Application.Auth;
using Crew.Domain.Entities;
using Crew.Domain.Enums;
using Crew.Tests.Support;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Crew.Tests;

public class ChatHubTests : IClassFixture<CrewApiFactory>
{
    private readonly CrewApiFactory _factory;

    public ChatHubTests(CrewApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task JoinEventGroup_AllowsMembersToConnect()
    {
        await _factory.ResetDatabaseAsync();

        var eventId = Guid.NewGuid();
        var groupId = eventId;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userProvisioning = scope.ServiceProvider.GetRequiredService<UserProvisioningService>();
            var user = await userProvisioning.EnsureUserAsync(CrewApiFactory.TestFirebaseUid, "Integration Tester", cancellationToken: CancellationToken.None);

            dbContext.ChatGroups.Add(new ChatGroup
            {
                Id = groupId,
                Scope = GroupScope.Event,
                EventId = eventId,
                CreatedAt = DateTimeOffset.UtcNow
            });

            dbContext.ChatMemberships.Add(new ChatMembership
            {
                GroupId = groupId,
                UserId = user.Id,
                Role = "member",
                JoinedAt = DateTimeOffset.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }

        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri("http://localhost/hubs/chat"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>(CrewApiFactory.TestToken);
            })
            .WithAutomaticReconnect()
            .Build();

        try
        {
            await connection.StartAsync();

            Func<Task> act = async () => await connection.InvokeAsync("JoinEventGroup", eventId, CancellationToken.None);
            await act.Should().NotThrowAsync();
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }
}
