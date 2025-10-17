using System.ComponentModel.DataAnnotations;

namespace Crew.Contracts.Chat;

public sealed class SetReactionRequest
{
    [Required]
    [MaxLength(32)]
    public string Emoji { get; init; } = null!;
}
