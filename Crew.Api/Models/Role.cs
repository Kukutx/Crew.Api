using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

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

/// <summary>
/// Enumerates the built-in role keys recognized by the system.
/// </summary>
public enum RoleKey
{
    /// <summary>
    /// Grants access to standard user features.
    /// </summary>
    [EnumMember(Value = "user")]
    User,

    /// <summary>
    /// Grants administrative permissions.
    /// </summary>
    [EnumMember(Value = "admin")]
    Admin,
}
