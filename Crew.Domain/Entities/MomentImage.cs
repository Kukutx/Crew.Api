namespace Crew.Domain.Entities;

public class MomentImage
{
    public Guid Id { get; set; }
    public Guid MomentId { get; set; }
    public string Url { get; set; } = null!;
    public int SortOrder { get; set; }

    public Moment Moment { get; set; } = null!;
}
