using System;
using System.Collections.Generic;
using Crew.Api.Extensions;

namespace Crew.Api.Models;

public enum UserIdentityLabel
{
    Visitor = 1,
    Participant = 2,
    Organizer = 3,
}

public static class UserIdentityLabelExtensions
{
    public static string ToStorageValue(this UserIdentityLabel label)
        => label.GetEnumMemberValue();

    public static UserIdentityLabel FromStorageValue(string? value)
    {
        if (TryFromStorageValue(value, out var label))
        {
            return label;
        }

        throw new InvalidOperationException($"Unsupported identity label value '{value}'.");
    }

    public static bool TryFromStorageValue(string? value, out UserIdentityLabel label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            label = UserIdentityLabel.Visitor;
            return true;
        }

        if (EnumExtensions.TryParseEnumMemberValue(value.Trim(), out UserIdentityLabel parsed))
        {
            label = parsed;
            return true;
        }

        label = UserIdentityLabel.Visitor;
        return false;
    }

    public static IReadOnlyCollection<string> AllStorageValues { get; } =
        EnumExtensions.GetEnumMemberValues<UserIdentityLabel>();
}
