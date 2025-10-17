using System;
using System.ComponentModel.DataAnnotations;
using Crew.Domain.Enums;

namespace Crew.Contracts.Chat;

public sealed class AddMemberRequest
{
    [Required]
    public Guid UserId { get; init; }

    public ChatMemberRole Role { get; init; } = ChatMemberRole.Member;
}
