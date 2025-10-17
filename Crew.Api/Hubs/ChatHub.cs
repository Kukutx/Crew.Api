using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Crew.Api.Mapping;
using Crew.Application.Chat;
using Crew.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Crew.Api.Hubs;

[Authorize]
public sealed class ChatHub : Hub
{
    private readonly ChatService _chatService;

    public ChatHub(ChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<Guid> JoinEventGroup(Guid eventId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            throw new HubException("Unauthorized");
        }

        var chat = await _chatService.GetEventChatAsync(eventId, cancellationToken);
        if (chat is null)
        {
            throw new HubException("Event group not found");
        }

        if (!await _chatService.IsMemberAsync(chat.Id, userId.Value, cancellationToken))
        {
            throw new HubException("Forbidden");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(chat.Id), cancellationToken);
        return chat.Id;
    }

    public async Task JoinChat(Guid chatId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            throw new HubException("Unauthorized");
        }

        if (!await _chatService.IsMemberAsync(chatId, userId.Value, cancellationToken))
        {
            throw new HubException("Forbidden");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(chatId), cancellationToken);
    }

    public async Task<Guid> OpenDialog(Guid userId, CancellationToken cancellationToken)
    {
        var currentUserId = GetUserId();
        if (currentUserId is null)
        {
            throw new HubException("Unauthorized");
        }

        var chat = await _chatService.OpenDirectChatAsync(currentUserId.Value, userId, cancellationToken);
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(chat.Id), cancellationToken);
        return chat.Id;
    }

    public async Task SendMessage(
        Guid chatId,
        ChatMessageKind kind,
        string? bodyText,
        string? metaJson,
        CancellationToken cancellationToken)
    {
        if (kind == ChatMessageKind.Text && string.IsNullOrWhiteSpace(bodyText))
        {
            throw new HubException("Message cannot be empty");
        }

        var userId = GetUserId();
        if (userId is null)
        {
            throw new HubException("Unauthorized");
        }

        if (!await _chatService.IsMemberAsync(chatId, userId.Value, cancellationToken))
        {
            throw new HubException("Forbidden");
        }

        var message = await _chatService.SendMessageAsync(chatId, userId.Value, kind, bodyText, metaJson, cancellationToken);
        await Clients.Group(GroupName(chatId)).SendAsync("MessageCreated", message.ToDto(), cancellationToken);
    }

    public async Task MarkRead(Guid chatId, long maxSeq, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            throw new HubException("Unauthorized");
        }

        await _chatService.MarkReadAsync(chatId, userId.Value, maxSeq, cancellationToken);
        await Clients.Group(GroupName(chatId)).SendAsync("Read", new { ChatId = chatId, UserId = userId.Value, MaxSeq = maxSeq }, cancellationToken);
    }

    public async Task Typing(Guid chatId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            throw new HubException("Unauthorized");
        }

        await Clients.OthersInGroup(GroupName(chatId)).SendAsync("Typing", new { ChatId = chatId, UserId = userId.Value, Timestamp = DateTimeOffset.UtcNow }, cancellationToken);
    }

    private Guid? GetUserId()
    {
        var value = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static string GroupName(Guid chatId) => $"chat:{chatId}";
}
