using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Crew.Api.Models;

public class Role
{
    public int Id { get; set; }

    [MaxLength(64)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    public bool IsSystemRole { get; set; } = true;

    public ICollection<UserRoleAssignment> UserRoles { get; set; } = new List<UserRoleAssignment>();
}

public static class RoleKeys
{
    public const string User = "user";
    public const string Admin = "admin";
}
