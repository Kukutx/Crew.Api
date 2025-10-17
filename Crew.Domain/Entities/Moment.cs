namespace Crew.Domain.Entities;

public class Moment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? EventId { get; set; }
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public string Country { get; set; } = null!;
    public string? City { get; set; }
    public string CoverImageUrl { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public RoadTripEvent? Event { get; set; }
    public ICollection<MomentImage> Images { get; set; } = new List<MomentImage>();
    public ICollection<MomentComment> Comments { get; set; } = new List<MomentComment>();
}
