using Crew.Application.Abstractions;
using Crew.Domain.Entities;
using Crew.Domain.Enums;
using Crew.Domain.Events;
using System.Text.Json;

namespace Crew.Application.Events;

public sealed class RegisterForEventCommand
{
    private readonly IRoadTripEventRepository _eventRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUserActivityHistoryRepository _activityHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterForEventCommand(
        IRoadTripEventRepository eventRepository,
        IRegistrationRepository registrationRepository,
        IChatRepository chatRepository,
        IOutboxRepository outboxRepository,
        IUserActivityHistoryRepository activityHistoryRepository,
        IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _registrationRepository = registrationRepository;
        _chatRepository = chatRepository;
        _outboxRepository = outboxRepository;
        _activityHistoryRepository = activityHistoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Registration> RegisterAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new InvalidOperationException("Event not found");

        var existing = await _registrationRepository.GetAsync(eventId, userId, cancellationToken);
        if (existing is not null)
        {
            if (existing.Status == RegistrationStatus.Confirmed)
            {
                await EnsureActivityHistoryAsync(userId, eventId, cancellationToken);
                return existing;
            }

            existing.Status = RegistrationStatus.Confirmed;
            existing.CreatedAt = DateTimeOffset.UtcNow;
            await EnsureActivityHistoryAsync(userId, eventId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = RegistrationStatus.Confirmed,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _registrationRepository.AddAsync(registration, cancellationToken);
        await EnsureActivityHistoryAsync(userId, eventId, cancellationToken);

        var group = await _chatRepository.GetEventGroupAsync(eventId, cancellationToken);
        if (group is null)
        {
            group = new ChatGroup
            {
                Id = eventId,
                Scope = GroupScope.Event,
                EventId = eventId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _chatRepository.AddGroupAsync(group, cancellationToken);
        }

        var membership = new ChatMembership
        {
            GroupId = group.Id,
            UserId = userId,
            Role = "member",
            JoinedAt = DateTimeOffset.UtcNow
        };

        await _chatRepository.AddMembershipAsync(membership, cancellationToken);

        var outboxEvent = new UserJoinedGroupEvent(group.Id, userId, membership.JoinedAt);
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(UserJoinedGroupEvent),
            Payload = JsonSerializer.Serialize(outboxEvent),
            OccurredAt = DateTimeOffset.UtcNow
        };

        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return registration;
    }

    public async Task CancelAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetAsync(eventId, userId, cancellationToken)
            ?? throw new InvalidOperationException("Registration not found");

        await _registrationRepository.RemoveAsync(registration);

        var history = await _activityHistoryRepository.FindAsync(userId, eventId, ActivityRole.Participant, cancellationToken);
        if (history is not null)
        {
            await _activityHistoryRepository.RemoveAsync(history, cancellationToken);
        }

        var group = await _chatRepository.GetEventGroupAsync(eventId, cancellationToken);
        if (group is not null)
        {
            var membership = await _chatRepository.GetMembershipAsync(group.Id, userId, cancellationToken);
            if (membership is not null)
            {
                await _chatRepository.RemoveMembershipAsync(membership);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureActivityHistoryAsync(Guid userId, Guid eventId, CancellationToken cancellationToken)
    {
        var history = await _activityHistoryRepository.FindAsync(userId, eventId, ActivityRole.Participant, cancellationToken);
        if (history is not null)
        {
            return;
        }

        var entry = new UserActivityHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventId = eventId,
            Role = ActivityRole.Participant,
            OccurredAt = DateTimeOffset.UtcNow
        };

        await _activityHistoryRepository.AddAsync(entry, cancellationToken);
    }
}
