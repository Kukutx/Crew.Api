using System;
using System.ComponentModel.DataAnnotations;

namespace Crew.Contracts.Chat;

public sealed class MarkReadRequest
{
    [Required]
    public Guid ChatId { get; init; }

    [Range(0, long.MaxValue)]
    public long MaxSeq { get; init; }
}
