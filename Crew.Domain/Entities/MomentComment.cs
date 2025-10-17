namespace Crew.Domain.Entities;

public class MomentComment
{
    public Guid Id { get; set; }
    public Guid MomentId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public Moment Moment { get; set; } = null!;
    public User Author { get; set; } = null!;
}
