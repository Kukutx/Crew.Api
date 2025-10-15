using System;
using System.Collections.Generic;

namespace Crew.Api.Models;

public enum UserIdentityLabel
{
    Visitor = 1,
    Participant = 2,
    Organizer = 3,
}

public static class UserIdentityLabelExtensions
{
    private static readonly IReadOnlyDictionary<string, UserIdentityLabel> LocalizedToEnum =
        new Dictionary<string, UserIdentityLabel>(StringComparer.Ordinal)
        {
            ["游客"] = UserIdentityLabel.Visitor,
            ["参与者"] = UserIdentityLabel.Participant,
            ["组织者"] = UserIdentityLabel.Organizer,
        };

    private static readonly IReadOnlyDictionary<UserIdentityLabel, string> EnumToLocalized =
        new Dictionary<UserIdentityLabel, string>
        {
            [UserIdentityLabel.Visitor] = "游客",
            [UserIdentityLabel.Participant] = "参与者",
            [UserIdentityLabel.Organizer] = "组织者",
        };

    public static string ToLocalizedString(this UserIdentityLabel label)
        => EnumToLocalized.TryGetValue(label, out var value)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(label), label, "Unknown identity label value.");

    public static UserIdentityLabel FromLocalizedString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return UserIdentityLabel.Visitor;
        }

        var trimmed = value.Trim();
        if (LocalizedToEnum.TryGetValue(trimmed, out var label))
        {
            return label;
        }

        throw new InvalidOperationException($"Unsupported identity label value '{value}'.");
    }

    public static bool TryFromLocalizedString(string? value, out UserIdentityLabel label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            label = UserIdentityLabel.Visitor;
            return true;
        }

        return LocalizedToEnum.TryGetValue(value.Trim(), out label);
    }

    public static IReadOnlyCollection<string> AllLocalizedStrings { get; } =
        Array.AsReadOnly(new[]
        {
            EnumToLocalized[UserIdentityLabel.Visitor],
            EnumToLocalized[UserIdentityLabel.Participant],
            EnumToLocalized[UserIdentityLabel.Organizer],
        });
}
