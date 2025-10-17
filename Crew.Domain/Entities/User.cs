using Crew.Domain.Enums;

namespace Crew.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirebaseUid { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<UserTag> Tags { get; set; } = new List<UserTag>();
    public ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();
    public ICollection<UserFollow> Following { get; set; } = new List<UserFollow>();
    public ICollection<UserActivityHistory> ActivityHistory { get; set; } = new List<UserActivityHistory>();
    public ICollection<UserGuestbookEntry> GuestbookEntries { get; set; } = new List<UserGuestbookEntry>();
    public ICollection<UserGuestbookEntry> AuthoredGuestbookEntries { get; set; } = new List<UserGuestbookEntry>();
    public ICollection<Moment> Moments { get; set; } = new List<Moment>();
}
