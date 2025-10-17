using System;
using Crew.Domain.Enums;

namespace Crew.Domain.Entities;

public class ChatMember
{
    public Guid ChatId { get; set; }
    public Guid UserId { get; set; }
    public ChatMemberRole Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? MutedUntil { get; set; }
    public long? LastReadMessageSeq { get; set; }
    public DateTimeOffset? LeftAt { get; set; }

    public Chat? Chat { get; set; }
    public User? User { get; set; }
}
