using Crew.Application.Abstractions;
using Crew.Domain.Entities;
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

    public Task<ChatGroup?> GetEventGroupAsync(Guid eventId, CancellationToken cancellationToken = default)
        => _dbContext.ChatGroups.FirstOrDefaultAsync(x => x.EventId == eventId, cancellationToken);

    public Task AddGroupAsync(ChatGroup group, CancellationToken cancellationToken = default)
        => _dbContext.ChatGroups.AddAsync(group, cancellationToken).AsTask();

    public Task AddMembershipAsync(ChatMembership membership, CancellationToken cancellationToken = default)
        => _dbContext.ChatMemberships.AddAsync(membership, cancellationToken).AsTask();

    public Task RemoveMembershipAsync(ChatMembership membership)
    {
        _dbContext.ChatMemberships.Remove(membership);
        return Task.CompletedTask;
    }

    public Task<ChatMembership?> GetMembershipAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default)
        => _dbContext.ChatMemberships.FirstOrDefaultAsync(x => x.GroupId == groupId && x.UserId == userId, cancellationToken);

    public Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
        => _dbContext.ChatMessages.AddAsync(message, cancellationToken).AsTask();

    public async Task<PrivateDialog?> GetOrCreateDialogAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default)
    {
        var ordered = OrderUsers(userA, userB);
        var dialog = await _dbContext.PrivateDialogs
            .FirstOrDefaultAsync(x => x.UserA == ordered.a && x.UserB == ordered.b, cancellationToken);

        if (dialog is not null)
        {
            return dialog;
        }

        dialog = new PrivateDialog
        {
            Id = Guid.NewGuid(),
            UserA = ordered.a,
            UserB = ordered.b,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.PrivateDialogs.AddAsync(dialog, cancellationToken);
        return dialog;
    }

    public Task<PrivateDialog?> GetDialogAsync(Guid dialogId, CancellationToken cancellationToken = default)
        => _dbContext.PrivateDialogs.FirstOrDefaultAsync(x => x.Id == dialogId, cancellationToken);

    public Task AddPrivateMessageAsync(PrivateMessage message, CancellationToken cancellationToken = default)
        => _dbContext.PrivateMessages.AddAsync(message, cancellationToken).AsTask();

    private static (Guid a, Guid b) OrderUsers(Guid first, Guid second)
        => first.CompareTo(second) <= 0 ? (first, second) : (second, first);
}
