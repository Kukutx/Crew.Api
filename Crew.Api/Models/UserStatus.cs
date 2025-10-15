using System;
using System.Collections.Generic;

namespace Crew.Api.Models;

public enum UserStatus
{
    Active = 1,
    Suspended = 2,
}

public static class UserStatusExtensions
{
    private static readonly IReadOnlyDictionary<string, UserStatus> StringToStatus =
        new Dictionary<string, UserStatus>(StringComparer.OrdinalIgnoreCase)
        {
            ["active"] = UserStatus.Active,
            ["suspended"] = UserStatus.Suspended,
        };

    private static readonly IReadOnlyDictionary<UserStatus, string> StatusToString =
        new Dictionary<UserStatus, string>
        {
            [UserStatus.Active] = "active",
            [UserStatus.Suspended] = "suspended",
        };

    public static string ToStorageValue(this UserStatus status)
        => StatusToString.TryGetValue(status, out var value)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown user status value.");

    public static UserStatus FromStorageValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return UserStatus.Active;
        }

        var trimmed = value.Trim();
        if (StringToStatus.TryGetValue(trimmed, out var status))
        {
            return status;
        }

        throw new InvalidOperationException($"Unsupported user status value '{value}'.");
    }

    public static bool TryParse(string? value, out UserStatus status)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            status = UserStatus.Active;
            return true;
        }

        return StringToStatus.TryGetValue(value.Trim(), out status);
    }

    public static IReadOnlyCollection<string> AllStorageValues { get; } =
        Array.AsReadOnly(new[]
        {
            StatusToString[UserStatus.Active],
            StatusToString[UserStatus.Suspended],
        });
}
