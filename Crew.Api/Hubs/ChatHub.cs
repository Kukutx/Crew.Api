using System.Security.Claims;
using Crew.Api.Mapping;
using Crew.Application.Chat;
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

    public async Task JoinEventGroup(Guid eventId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            throw new HubException("Unauthorized");
        }

        var group = await _chatService.GetEventGroupAsync(eventId, cancellationToken);
        if (group is null)
        {
            throw new HubException("Event group not found");
        }

        if (!await _chatService.IsMemberAsync(group.Id, userId.Value, cancellationToken))
        {
            throw new HubException("Forbidden");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(group.Id), cancellationToken);
    }

    public async Task SendToGroup(Guid groupId, string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new HubException("Message cannot be empty");
        }

        var userId = GetUserId();
        if (userId is null)
        {
            throw new HubException("Unauthorized");
        }

        if (!await _chatService.IsMemberAsync(groupId, userId.Value, cancellationToken))
        {
            throw new HubException("Forbidden");
        }

        var message = await _chatService.SendToGroupAsync(groupId, userId.Value, text, cancellationToken);
        await Clients.Group(GroupName(groupId)).SendAsync("msg", message.ToDto(), cancellationToken);
    }

    public async Task<Guid> OpenDialog(Guid userId, CancellationToken cancellationToken)
    {
        var currentUserId = GetUserId();
        if (currentUserId is null)
        {
            throw new HubException("Unauthorized");
        }

        var dialog = await _chatService.OpenDialogAsync(currentUserId.Value, userId, cancellationToken);
        return dialog.Id;
    }

    public async Task SendPrivate(Guid dialogId, string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new HubException("Message cannot be empty");
        }

        var userId = GetUserId();
        if (userId is null)
        {
            throw new HubException("Unauthorized");
        }

        var dialog = await _chatService.GetDialogAsync(dialogId, cancellationToken);
        if (dialog is null || (dialog.UserA != userId.Value && dialog.UserB != userId.Value))
        {
            throw new HubException("Forbidden");
        }

        var message = await _chatService.SendPrivateAsync(dialogId, userId.Value, text, cancellationToken);
        var recipients = new[] { dialog.UserA.ToString(), dialog.UserB.ToString() };
        await Clients.Users(recipients).SendAsync("pm", message.ToDto(), cancellationToken);
    }

    private Guid? GetUserId()
    {
        var value = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static string GroupName(Guid groupId) => $"group:{groupId}";
}
