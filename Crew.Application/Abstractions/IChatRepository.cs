using Crew.Domain.Entities;

namespace Crew.Application.Abstractions;

public interface IChatRepository
{
    Task<ChatGroup?> GetEventGroupAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task AddGroupAsync(ChatGroup group, CancellationToken cancellationToken = default);
    Task AddMembershipAsync(ChatMembership membership, CancellationToken cancellationToken = default);
    Task RemoveMembershipAsync(ChatMembership membership);
    Task<ChatMembership?> GetMembershipAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default);
    Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task<PrivateDialog?> GetOrCreateDialogAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
    Task<PrivateDialog?> GetDialogAsync(Guid dialogId, CancellationToken cancellationToken = default);
    Task AddPrivateMessageAsync(PrivateMessage message, CancellationToken cancellationToken = default);
}
