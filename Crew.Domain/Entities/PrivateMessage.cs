namespace Crew.Domain.Entities;

public class PrivateMessage
{
    public Guid Id { get; set; }
    public Guid DialogId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = null!;
    public DateTimeOffset SentAt { get; set; }
    public PrivateDialog? Dialog { get; set; }
    public User? Sender { get; set; }
}
