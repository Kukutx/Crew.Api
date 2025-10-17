using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Crew.Api.Hubs;
using Crew.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Crew.Api.Messaging;

public sealed class UserJoinedGroupHandler : IOutboxEventHandler
{
    private const string EventType = "UserJoinedGroupEvent";
    private readonly IHubContext<ChatHub> _hubContext;

    public UserJoinedGroupHandler(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public bool CanHandle(string type) => type == EventType;

    public async Task HandleAsync(string payload, CancellationToken cancellationToken = default)
    {
        var @event = JsonSerializer.Deserialize<UserJoinedGroupPayload>(payload);
        if (@event is null)
        {
            return;
        }

        await _hubContext.Clients.Group(GroupName(@event.ChatId)).SendAsync("system", new
        {
            userId = @event.UserId,
            joinedAt = @event.JoinedAt
        }, cancellationToken);
    }

    private static string GroupName(Guid chatId) => $"chat:{chatId}";

    private sealed record UserJoinedGroupPayload(Guid ChatId, Guid UserId, DateTimeOffset JoinedAt);
}
