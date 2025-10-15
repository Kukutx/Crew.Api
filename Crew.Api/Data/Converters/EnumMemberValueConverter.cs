using System;
using Crew.Api.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Crew.Api.Data.Converters;

public sealed class EnumMemberValueConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public EnumMemberValueConverter()
        : base(
            value => value.GetEnumMemberValue(),
            value => EnumExtensions.ParseEnumMemberValue<TEnum>(value))
    {
    }
}
