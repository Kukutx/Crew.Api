using Crew.Domain.Enums;

namespace Crew.Domain.Entities;

public class Registration
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public RegistrationStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public RoadTripEvent? Event { get; set; }
    public User? User { get; set; }
}
