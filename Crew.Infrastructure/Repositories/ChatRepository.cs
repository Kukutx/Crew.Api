using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crew.Application.Abstractions;
using Crew.Domain.Entities;
using Crew.Domain.Enums;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crew.Infrastructure.Repositories;

internal sealed class ChatRepository : IChatRepository
{
    private readonly AppDbContext _dbContext;

    public ChatRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Chat?> GetChatAsync(Guid chatId, CancellationToken cancellationToken = default)
        => _dbContext.Chats.FirstOrDefaultAsync(x => x.Id == chatId, cancellationToken);

    public Task<Chat?> GetEventChatAsync(Guid eventId, CancellationToken cancellationToken = default)
        => _dbContext.Chats.FirstOrDefaultAsync(x => x.EventId == eventId && x.Type == ChatType.EventGroup, cancellationToken);

    public async Task<Chat?> GetDirectChatAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Chats
            .Where(x => x.Type == ChatType.Direct)
            .Where(x => _dbContext.ChatMembers.Any(m => m.ChatId == x.Id && m.UserId == userA && m.LeftAt == null))
            .Where(x => _dbContext.ChatMembers.Any(m => m.ChatId == x.Id && m.UserId == userB && m.LeftAt == null))
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(Chat chat, ChatMember membership, ChatMessage? lastMessage, long lastSeq)>> GetChatSummariesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = from member in _dbContext.ChatMembers.AsNoTracking()
                    where member.UserId == userId && member.LeftAt == null
                    join chat in _dbContext.Chats.AsNoTracking() on member.ChatId equals chat.Id
                    select new
                    {
                        chat,
                        membership = member,
                        lastMessage = _dbContext.ChatMessages
                            .AsNoTracking()
                            .Where(m => m.ChatId == chat.Id)
                            .OrderByDescending(m => m.Seq)
                            .FirstOrDefault(),
                        lastSeq = _dbContext.ChatMessages
                            .Where(m => m.ChatId == chat.Id)
                            .Select(m => (long?)m.Seq)
                            .OrderByDescending(seq => seq)
                            .FirstOrDefault() ?? 0L
                    };

        var data = await query.ToListAsync(cancellationToken);
        return [.. data.Select(x => (x.chat, x.membership, x.lastMessage, x.lastSeq))];
    }

    public Task AddChatAsync(Chat chat, CancellationToken cancellationToken = default)
        => _dbContext.Chats.AddAsync(chat, cancellationToken).AsTask();

    public Task UpdateChatAsync(Chat chat)
    {
        _dbContext.Chats.Update(chat);
        return Task.CompletedTask;
    }

    public Task<ChatMember?> GetMembershipAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
        => _dbContext.ChatMembers.FirstOrDefaultAsync(x => x.ChatId == chatId && x.UserId == userId, cancellationToken);

    public Task AddMemberAsync(ChatMember member, CancellationToken cancellationToken = default)
        => _dbContext.ChatMembers.AddAsync(member, cancellationToken).AsTask();

    public Task UpdateMemberAsync(ChatMember member)
    {
        _dbContext.ChatMembers.Update(member);
        return Task.CompletedTask;
    }

    public Task RemoveMemberAsync(ChatMember member)
    {
        _dbContext.ChatMembers.Update(member);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<ChatMember>> GetMembersAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        var members = await _dbContext.ChatMembers
            .Where(x => x.ChatId == chatId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return members;
    }

    public async Task<long> GetNextMessageSeqAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        var maxSeq = await _dbContext.ChatMessages
            .Where(x => x.ChatId == chatId)
            .Select(x => (long?)x.Seq)
            .MaxAsync(cancellationToken);

        return (maxSeq ?? 0L) + 1L;
    }

    public async Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        await _dbContext.ChatMessages.AddAsync(message, cancellationToken);
    }

    public Task<ChatMessage?> GetMessageAsync(long messageId, CancellationToken cancellationToken = default)
        => _dbContext.ChatMessages
            .Include(x => x.Attachments)
            .Include(x => x.Reactions)
            .FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid chatId, long? beforeSeq, int limit, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ChatMessages
            .AsNoTracking()
            .Include(x => x.Attachments)
            .Include(x => x.Reactions)
            .Where(x => x.ChatId == chatId);

        if (beforeSeq.HasValue)
        {
            query = query.Where(x => x.Seq < beforeSeq.Value);
        }

        return await query
            .OrderByDescending(x => x.Seq)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public Task AddReactionAsync(ChatMessageReaction reaction, CancellationToken cancellationToken = default)
        => _dbContext.ChatMessageReactions.AddAsync(reaction, cancellationToken).AsTask();

    public Task RemoveReactionAsync(ChatMessageReaction reaction)
    {
        _dbContext.ChatMessageReactions.Remove(reaction);
        return Task.CompletedTask;
    }

    public Task<ChatMessageReaction?> GetReactionAsync(long messageId, Guid userId, string emoji, CancellationToken cancellationToken = default)
        => _dbContext.ChatMessageReactions.FirstOrDefaultAsync(x => x.MessageId == messageId && x.UserId == userId && x.Emoji == emoji, cancellationToken);

    public async Task<IReadOnlyList<ChatMessage>> SearchMessagesAsync(Guid userId, string query, Guid? chatId, int limit, CancellationToken cancellationToken = default)
    {
        var accessibleChatIds = _dbContext.ChatMembers
            .Where(x => x.UserId == userId && x.LeftAt == null)
            .Select(x => x.ChatId);

        var messages = _dbContext.ChatMessages
            .AsNoTracking()
            .Include(x => x.Attachments)
            .Include(x => x.Reactions)
            .Where(x => accessibleChatIds.Contains(x.ChatId));

        if (chatId.HasValue)
        {
            messages = messages.Where(x => x.ChatId == chatId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query}%";
            messages = messages.Where(x => x.BodyText != null && EF.Functions.ILike(x.BodyText, pattern));
        }

        return await messages
            .OrderByDescending(x => x.Seq)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
