namespace Crew.Api.Models;

public class DomainUsers
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Uid { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Avatar { get; set; } = AvatarDefaults.FallbackUrl;
    public string Cover { get; set; } = string.Empty;
    public int Followers { get; set; } = 0;
    public int Following { get; set; } = 0;
    public int Likes { get; set; } = 0;
    public bool Followed { get; set; } = false;
    public UserRole Role { get; set; } = UserRole.User;
    public int? SubscriptionPlanId { get; set; }
    public SubscriptionPlan? SubscriptionPlan { get; set; }
}
