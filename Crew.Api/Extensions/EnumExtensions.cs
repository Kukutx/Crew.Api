using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Crew.Api.Extensions;

public static class EnumExtensions
{
    private static class EnumCache<TEnum>
        where TEnum : struct, Enum
    {
        public static readonly IReadOnlyDictionary<TEnum, string> EnumToString;
        public static readonly IReadOnlyDictionary<string, TEnum> StringToEnum;
        public static readonly IReadOnlyCollection<string> AllValues;

        static EnumCache()
        {
            var enumToString = new Dictionary<TEnum, string>();
            var stringToEnum = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);

            foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var value = (TEnum)field.GetValue(null)!;
                var enumMember = field.GetCustomAttribute<EnumMemberAttribute>();
                var stringValue = enumMember?.Value ?? field.Name;

                enumToString[value] = stringValue;

                if (!stringToEnum.ContainsKey(stringValue))
                {
                    stringToEnum[stringValue] = value;
                }

                if (!stringToEnum.ContainsKey(field.Name))
                {
                    stringToEnum[field.Name] = value;
                }
            }

            EnumToString = enumToString;
            StringToEnum = stringToEnum;
            AllValues = enumToString.Values.ToArray();
        }
    }

    public static string GetEnumMemberValue<TEnum>(this TEnum value)
        where TEnum : struct, Enum
        => EnumCache<TEnum>.EnumToString.TryGetValue(value, out var stringValue)
            ? stringValue
            : value.ToString();

    public static IReadOnlyCollection<string> GetEnumMemberValues<TEnum>()
        where TEnum : struct, Enum
        => EnumCache<TEnum>.AllValues;

    public static bool TryParseEnumMemberValue<TEnum>(string? value, out TEnum result)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
        {
            result = default;
            return false;
        }

        return EnumCache<TEnum>.StringToEnum.TryGetValue(value, out result);
    }

    public static TEnum ParseEnumMemberValue<TEnum>(string value)
        where TEnum : struct, Enum
    {
        if (TryParseEnumMemberValue<TEnum>(value, out var result))
        {
            return result;
        }

        throw new ArgumentException($"'{value}' is not a valid {typeof(TEnum).Name} value.", nameof(value));
    }
}
