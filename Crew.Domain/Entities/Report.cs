using System;
using Crew.Domain.Enums;

namespace Crew.Domain.Entities;

public class Report
{
    public Guid Id { get; set; }
    public ReportTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = null!;
    public Guid ReporterId { get; set; }
    public ReportStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
