namespace Crew.Domain.Entities;

public class UserGuestbookEntry
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = null!;
    public int? Rating { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User Owner { get; set; } = null!;
    public User Author { get; set; } = null!;
}
