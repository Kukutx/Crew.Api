using Crew.Application.Abstractions;
using Crew.Domain.Entities;

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

    public Task<ChatGroup?> GetEventGroupAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return _chatRepository.GetEventGroupAsync(eventId, cancellationToken);
    }

    public async Task<bool> IsMemberAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default)
    {
        var membership = await _chatRepository.GetMembershipAsync(groupId, userId, cancellationToken);
        return membership is not null;
    }

    public async Task<ChatMessage> SendToGroupAsync(Guid groupId, Guid senderId, string content, CancellationToken cancellationToken = default)
    {
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            SenderId = senderId,
            Content = content,
            SentAt = DateTimeOffset.UtcNow
        };

        await _chatRepository.AddMessageAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return message;
    }

    public Task<PrivateDialog?> GetDialogAsync(Guid dialogId, CancellationToken cancellationToken = default)
    {
        return _chatRepository.GetDialogAsync(dialogId, cancellationToken);
    }

    public async Task<PrivateDialog> OpenDialogAsync(Guid requesterId, Guid counterpartId, CancellationToken cancellationToken = default)
    {
        var dialog = await _chatRepository.GetOrCreateDialogAsync(requesterId, counterpartId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return dialog;
    }

    public async Task<PrivateMessage> SendPrivateAsync(Guid dialogId, Guid senderId, string content, CancellationToken cancellationToken = default)
    {
        var message = new PrivateMessage
        {
            Id = Guid.NewGuid(),
            DialogId = dialogId,
            SenderId = senderId,
            Content = content,
            SentAt = DateTimeOffset.UtcNow
        };

        await _chatRepository.AddPrivateMessageAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return message;
    }
}
