using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crew.Api.Models;

/// <summary>
/// Represents the status of a <see cref="UserAccount"/>.
/// </summary>
[JsonConverter(typeof(UserStatusJsonConverter))]
public readonly struct UserStatus : IEquatable<UserStatus>
{
    private static readonly IReadOnlyDictionary<string, UserStatus> KnownStatuses =
        new Dictionary<string, UserStatus>(StringComparer.OrdinalIgnoreCase)
        {
            ["active"] = new UserStatus("active"),
            ["suspended"] = new UserStatus("suspended"),
        };

    private readonly string _value;

    private UserStatus(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the string representation stored in the database.
    /// </summary>
    public string Value => _value ?? string.Empty;

    public static UserStatus Active => KnownStatuses["active"];

    public static UserStatus Suspended => KnownStatuses["suspended"];

    /// <summary>
    /// Creates a <see cref="UserStatus"/> from a database value.
    /// </summary>
    public static UserStatus From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("User status cannot be empty.", nameof(value));
        }

        value = value.Trim();

        if (KnownStatuses.TryGetValue(value, out var status))
        {
            return status;
        }

        var normalized = value.ToLowerInvariant();
        return new UserStatus(normalized);
    }

    public bool Equals(UserStatus other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is UserStatus other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => Value;

    public static implicit operator string(UserStatus status) => status.Value;

    public static implicit operator UserStatus(string value) => From(value);

    private sealed class UserStatusJsonConverter : JsonConverter<UserStatus>
    {
        public override UserStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Unexpected token parsing UserStatus. Expected String, got {reader.TokenType}.");
            }

            var value = reader.GetString();
            return value is null ? Active : From(value);
        }

        public override void Write(Utf8JsonWriter writer, UserStatus value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
