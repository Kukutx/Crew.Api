using System.ComponentModel.DataAnnotations;
using Crew.Domain.Enums;

namespace Crew.Contracts.Chat;

public sealed class SendMessageRequest
{
    [Required]
    public ChatMessageKind Kind { get; init; }

    public string? BodyText { get; init; }

    public string? MetaJson { get; init; }
}
