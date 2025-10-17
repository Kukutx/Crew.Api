using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crew.Domain.Entities;

namespace Crew.Application.Abstractions;

public interface IChatRepository
{
    Task<Chat?> GetChatAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task<Chat?> GetEventChatAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<Chat?> GetDirectChatAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(Chat chat, ChatMember membership, ChatMessage? lastMessage, long lastSeq)>> GetChatSummariesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddChatAsync(Chat chat, CancellationToken cancellationToken = default);
    Task UpdateChatAsync(Chat chat);

    Task<ChatMember?> GetMembershipAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default);
    Task AddMemberAsync(ChatMember member, CancellationToken cancellationToken = default);
    Task UpdateMemberAsync(ChatMember member);
    Task RemoveMemberAsync(ChatMember member);
    Task<IReadOnlyList<ChatMember>> GetMembersAsync(Guid chatId, CancellationToken cancellationToken = default);

    Task<long> GetNextMessageSeqAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task<ChatMessage?> GetMessageAsync(long messageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid chatId, long? beforeSeq, int limit, CancellationToken cancellationToken = default);

    Task AddReactionAsync(ChatMessageReaction reaction, CancellationToken cancellationToken = default);
    Task RemoveReactionAsync(ChatMessageReaction reaction);
    Task<ChatMessageReaction?> GetReactionAsync(long messageId, Guid userId, string emoji, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatMessage>> SearchMessagesAsync(Guid userId, string query, Guid? chatId, int limit, CancellationToken cancellationToken = default);
}
