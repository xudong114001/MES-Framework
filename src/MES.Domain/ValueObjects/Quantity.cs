using MES.Domain.Exceptions;

namespace MES.Domain.ValueObjects;

/// <summary>
/// 数量值对象 - 封装数量值与单位，确保业务不变式（非负数、同单位运算）
/// </summary>
public record Quantity
{
    public decimal Value { get; }
    public string Unit { get; }

    public Quantity(decimal value, string unit = "个")
    {
        if (value < 0)
            throw new DomainException("数量不能为负数");
        if (string.IsNullOrWhiteSpace(unit))
            throw new DomainException("单位不能为空");

        Value = value;
        Unit = unit;
    }

    public static Quantity operator +(Quantity a, Quantity b)
    {
        if (a.Unit != b.Unit)
            throw new DomainException("单位不同不能相加");
        return new Quantity(a.Value + b.Value, a.Unit);
    }

    public static Quantity operator -(Quantity a, Quantity b)
    {
        if (a.Unit != b.Unit)
            throw new DomainException("单位不同不能相减");
        return new Quantity(a.Value - b.Value, a.Unit);
    }

    public bool IsZero => Value == 0;
    public bool IsPositive => Value > 0;
    public bool IsNegative => Value < 0;

    /// <summary>
    /// 创建零数量值对象
    /// </summary>
    public static Quantity Zero(string unit = "个") => new(0, unit);

    /// <summary>
    /// 隐式转换为 decimal，便于与现有代码兼容
    /// </summary>
    public static implicit operator decimal(Quantity quantity) => quantity.Value;

    public override string ToString() => $"{Value} {Unit}";
}
