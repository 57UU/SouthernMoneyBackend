using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace Database;

/// <summary>
/// DateTime转换器，确保所有DateTime值以UTC格式存储和读取
/// </summary>
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            // 转换为存储格式：确保DateTime是UTC时间
            v => v.ToUniversalTime(),
            // 从存储格式转换：读取时指定为UTC时间
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}

/// <summary>
/// 可空DateTime转换器，确保所有可空DateTime值以UTC格式存储和读取
/// </summary>
public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter()
        : base(
            // 转换为存储格式：确保DateTime是UTC时间
            v => v.HasValue ? v.Value.ToUniversalTime() : null,
            // 从存储格式转换：读取时指定为UTC时间
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null)
    {
    }
}