using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crew.Application.Abstractions;
using Crew.Domain.Entities;
using Crew.Domain.Enums;

namespace Crew.Application.Chat;

public sealed class ChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChatService(IChatRepository chatRepository, IUnitOfWork unitOfWork)
    {
        _chatRepository = chatRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<Chat?> GetChatAsync(Guid chatId, CancellationToken cancellationToken = default)
        => _chatRepository.GetChatAsync(chatId, cancellationToken);

    public Task<Chat?> GetEventChatAsync(Guid eventId, CancellationToken cancellationToken = default)
        => _chatRepository.GetEventChatAsync(eventId, cancellationToken);

    public async Task<Chat> EnsureEventChatAsync(Guid eventId, Guid ownerId, string title, CancellationToken cancellationToken = default)
    {
        var chat = await _chatRepository.GetEventChatAsync(eventId, cancellationToken);
        if (chat is not null)
        {
            return chat;
        }

        chat = new Chat
        {
            Id = Guid.NewGuid(),
            Type = ChatType.EventGroup,
            EventId = eventId,
            OwnerUserId = ownerId,
            Title = title,
            CreatedAt = DateTimeOffset.UtcNow,
            IsArchived = false
        };

        await _chatRepository.AddChatAsync(chat, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return chat;
    }

    public async Task<bool> IsMemberAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _chatRepository.GetMembershipAsync(chatId, userId, cancellationToken);
        return member is not null && member.LeftAt is null;
    }

    public Task<ChatMember?> GetMembershipAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
        => _chatRepository.GetMembershipAsync(chatId, userId, cancellationToken);

    public Task<IReadOnlyList<ChatMember>> GetMembersAsync(Guid chatId, CancellationToken cancellationToken = default)
        => _chatRepository.GetMembersAsync(chatId, cancellationToken);

    public async Task<ChatMember> EnsureMemberAsync(Guid chatId, Guid userId, ChatMemberRole role, CancellationToken cancellationToken = default)
    {
        var member = await _chatRepository.GetMembershipAsync(chatId, userId, cancellationToken);
        if (member is not null)
        {
            if (member.LeftAt is not null)
            {
                member.LeftAt = null;
                member.JoinedAt = DateTimeOffset.UtcNow;
                member.Role = role;
                await _chatRepository.UpdateMemberAsync(member);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return member;
        }

        member = new ChatMember
        {
            ChatId = chatId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTimeOffset.UtcNow
        };

        await _chatRepository.AddMemberAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return member;
    }

    public async Task<Chat> OpenDirectChatAsync(Guid requesterId, Guid counterpartId, CancellationToken cancellationToken = default)
    {
        var existing = await _chatRepository.GetDirectChatAsync(requesterId, counterpartId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Type = ChatType.Direct,
            CreatedAt = DateTimeOffset.UtcNow,
            OwnerUserId = null,
            Title = null
        };

        await _chatRepository.AddChatAsync(chat, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await EnsureMemberAsync(chat.Id, requesterId, ChatMemberRole.Owner, cancellationToken);
        await EnsureMemberAsync(chat.Id, counterpartId, ChatMemberRole.Owner, cancellationToken);

        return chat;
    }

    public async Task<ChatMessage> SendMessageAsync(
        Guid chatId,
        Guid senderId,
        ChatMessageKind kind,
        string? bodyText,
        string? metaJson,
        CancellationToken cancellationToken = default)
    {
        var seq = await _chatRepository.GetNextMessageSeqAsync(chatId, cancellationToken);
        var message = new ChatMessage
        {
            ChatId = chatId,
            SenderId = senderId,
            Kind = kind,
            BodyText = bodyText,
            MetaJson = metaJson,
            CreatedAt = DateTimeOffset.UtcNow,
            Seq = seq,
            Status = ChatMessageStatus.Persisted
        };

        await _chatRepository.AddMessageAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return message;
    }

    public Task<IReadOnlyList<(Chat chat, ChatMember membership, ChatMessage? lastMessage, long lastSeq)>> GetChatSummariesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => _chatRepository.GetChatSummariesAsync(userId, cancellationToken);

    public Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid chatId, long? beforeSeq, int limit, CancellationToken cancellationToken = default)
        => _chatRepository.GetMessagesAsync(chatId, beforeSeq, limit, cancellationToken);

    public Task<ChatMessage?> GetMessageAsync(long messageId, CancellationToken cancellationToken = default)
        => _chatRepository.GetMessageAsync(messageId, cancellationToken);

    public async Task LeaveChatAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _chatRepository.GetMembershipAsync(chatId, userId, cancellationToken);
        if (member is null)
        {
            return;
        }

        member.LeftAt = DateTimeOffset.UtcNow;
        await _chatRepository.UpdateMemberAsync(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkReadAsync(Guid chatId, Guid userId, long maxSeq, CancellationToken cancellationToken = default)
    {
        var member = await _chatRepository.GetMembershipAsync(chatId, userId, cancellationToken);
        if (member is null)
        {
            throw new InvalidOperationException("Membership not found");
        }

        if (member.LastReadMessageSeq is null || member.LastReadMessageSeq < maxSeq)
        {
            member.LastReadMessageSeq = maxSeq;
            await _chatRepository.UpdateMemberAsync(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddReactionAsync(long messageId, Guid userId, string emoji, CancellationToken cancellationToken = default)
    {
        var existing = await _chatRepository.GetReactionAsync(messageId, userId, emoji, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var reaction = new ChatMessageReaction
        {
            MessageId = messageId,
            UserId = userId,
            Emoji = emoji,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _chatRepository.AddReactionAsync(reaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveReactionAsync(long messageId, Guid userId, string emoji, CancellationToken cancellationToken = default)
    {
        var existing = await _chatRepository.GetReactionAsync(messageId, userId, emoji, cancellationToken);
        if (existing is null)
        {
            return;
        }

        await _chatRepository.RemoveReactionAsync(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<ChatMessage>> SearchMessagesAsync(Guid userId, string query, Guid? chatId, int limit, CancellationToken cancellationToken = default)
        => _chatRepository.SearchMessagesAsync(userId, query, chatId, limit, cancellationToken);
}
