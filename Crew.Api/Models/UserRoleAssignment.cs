using System;

namespace Crew.Api.Models;

public class UserRoleAssignment
{
    public string UserUid { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    public UserAccount? User { get; set; }

    public Role? Role { get; set; }
}
