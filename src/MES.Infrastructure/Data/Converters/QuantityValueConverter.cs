using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MES.Domain.ValueObjects;

namespace MES.Infrastructure.Data.Converters;

/// <summary>
/// Quantity 值对象与数据库 decimal 列之间的转换器。
/// 当前数据库模式仅存储 decimal 值，Unit 默认为 "个"。
/// 当实体属性类型从 decimal 迁移为 Quantity 时，在 EF Core 配置中使用此转换器。
/// </summary>
public class QuantityValueConverter : ValueConverter<Quantity, decimal>
{
    public QuantityValueConverter()
        : base(
            quantity => quantity.Value,
            value => new Quantity(value))
    {
    }
}
