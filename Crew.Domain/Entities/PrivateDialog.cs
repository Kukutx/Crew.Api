namespace Crew.Domain.Entities;

public class PrivateDialog
{
    public Guid Id { get; set; }
    public Guid UserA { get; set; }
    public Guid UserB { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<PrivateMessage> Messages { get; set; } = new List<PrivateMessage>();
}
