using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Crew.Api.Mapping;
using Crew.Application.Chat;
using Crew.Contracts.Chat;
using Crew.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crew.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/chats")]
[Authorize]
public sealed class ChatsController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatsController(ChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ChatSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetChatsAsync(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var summaries = await _chatService.GetChatSummariesAsync(userId.Value, cancellationToken);
        var dto = summaries.Select(x => x.chat.ToSummaryDto(x.membership, x.lastMessage, x.lastSeq)).ToList();
        return Ok(dto);
    }

    [HttpGet("{id:guid}/messages")]
    [ProducesResponseType(typeof(IReadOnlyList<ChatMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessagesAsync(
        Guid id,
        [FromQuery] long? beforeSeq,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var chat = await _chatService.GetChatAsync(id, cancellationToken);
        if (chat is null)
        {
            return NotFound();
        }

        if (!await _chatService.IsMemberAsync(id, userId.Value, cancellationToken))
        {
            return Forbid();
        }

        limit = Math.Clamp(limit, 1, 100);
        var messages = await _chatService.GetMessagesAsync(id, beforeSeq, limit, cancellationToken);
        return Ok(messages.Select(m => m.ToDto()).ToList());
    }

    [HttpPost("{id:guid}/messages")]
    [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessageAsync(
        Guid id,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var chat = await _chatService.GetChatAsync(id, cancellationToken);
        if (chat is null)
        {
            return NotFound();
        }

        if (chat.IsArchived)
        {
            return BadRequest("Chat is archived");
        }

        if (!await _chatService.IsMemberAsync(id, userId.Value, cancellationToken))
        {
            return Forbid();
        }

        if (request.Kind == ChatMessageKind.Text && string.IsNullOrWhiteSpace(request.BodyText))
        {
            return BadRequest("Message cannot be empty");
        }

        var message = await _chatService.SendMessageAsync(id, userId.Value, request.Kind, request.BodyText, request.MetaJson, cancellationToken);
        return Ok(message.ToDto());
    }

    [HttpPost("{id:guid}/members")]
    [ProducesResponseType(typeof(ChatMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> JoinAsync(
        Guid id,
        [FromBody] AddMemberRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var currentUserId = GetUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var chat = await _chatService.GetChatAsync(id, cancellationToken);
        if (chat is null)
        {
            return NotFound();
        }

        if (request.UserId != currentUserId.Value)
        {
            var currentMembership = await _chatService.GetMembershipAsync(id, currentUserId.Value, cancellationToken);
            if (currentMembership is null || currentMembership.LeftAt is not null)
            {
                return Forbid();
            }

            if (currentMembership.Role is not ChatMemberRole.Owner and not ChatMemberRole.Admin)
            {
                return Forbid();
            }
        }

        var member = await _chatService.EnsureMemberAsync(id, request.UserId, request.Role, cancellationToken);
        return Ok(member.ToDto());
    }

    [HttpGet("{id:guid}/members")]
    [ProducesResponseType(typeof(IReadOnlyList<ChatMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMembersAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var chat = await _chatService.GetChatAsync(id, cancellationToken);
        if (chat is null)
        {
            return NotFound();
        }

        if (!await _chatService.IsMemberAsync(id, userId.Value, cancellationToken))
        {
            return Forbid();
        }

        var members = await _chatService.GetMembersAsync(id, cancellationToken);
        return Ok(members.Select(m => m.ToDto()).ToList());
    }

    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LeaveAsync(Guid id, Guid memberId, CancellationToken cancellationToken)
    {
        var currentUserId = GetUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var chat = await _chatService.GetChatAsync(id, cancellationToken);
        if (chat is null)
        {
            return NotFound();
        }

        if (currentUserId != memberId)
        {
            var membership = await _chatService.GetMembershipAsync(id, currentUserId.Value, cancellationToken);
            if (membership is null || membership.LeftAt is not null)
            {
                return Forbid();
            }

            if (membership.Role is not ChatMemberRole.Owner and not ChatMemberRole.Admin)
            {
                return Forbid();
            }
        }

        await _chatService.LeaveChatAsync(id, memberId, cancellationToken);
        return NoContent();
    }

    [HttpPost("~/api/v{version:apiVersion}/messages/{messageId:long}/reactions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddReactionAsync(
        long messageId,
        [FromBody] SetReactionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var message = await _chatService.GetMessageAsync(messageId, cancellationToken);
        if (message is null)
        {
            return NotFound();
        }

        if (!await _chatService.IsMemberAsync(message.ChatId, userId.Value, cancellationToken))
        {
            return Forbid();
        }

        await _chatService.AddReactionAsync(messageId, userId.Value, request.Emoji, cancellationToken);
        return NoContent();
    }

    [HttpDelete("~/api/v{version:apiVersion}/messages/{messageId:long}/reactions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveReactionAsync(
        long messageId,
        [FromQuery] string emoji,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(emoji))
        {
            return BadRequest("Emoji is required");
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var message = await _chatService.GetMessageAsync(messageId, cancellationToken);
        if (message is null)
        {
            return NotFound();
        }

        if (!await _chatService.IsMemberAsync(message.ChatId, userId.Value, cancellationToken))
        {
            return Forbid();
        }

        await _chatService.RemoveReactionAsync(messageId, userId.Value, emoji, cancellationToken);
        return NoContent();
    }

    [HttpPost("~/api/v{version:apiVersion}/messages/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkReadAsync([FromBody] MarkReadRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (!await _chatService.IsMemberAsync(request.ChatId, userId.Value, cancellationToken))
        {
            return Forbid();
        }

        await _chatService.MarkReadAsync(request.ChatId, userId.Value, request.MaxSeq, cancellationToken);
        return NoContent();
    }

    [HttpGet("~/api/v{version:apiVersion}/search/messages")]
    [ProducesResponseType(typeof(IReadOnlyList<ChatMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchMessagesAsync(
        [FromQuery] string q,
        [FromQuery] Guid? chatId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        limit = Math.Clamp(limit, 1, 100);
        var messages = await _chatService.SearchMessagesAsync(userId.Value, q, chatId, limit, cancellationToken);
        return Ok(messages.Select(m => m.ToDto()).ToList());
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
