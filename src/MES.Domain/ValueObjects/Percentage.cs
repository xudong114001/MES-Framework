using MES.Domain.Exceptions;

namespace MES.Domain.ValueObjects;

/// <summary>
/// 百分比值对象 - 封装 0.0~1.0 范围的比例值，确保业务不变式
/// </summary>
public record Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        if (value < 0 || value > 1)
            throw new DomainException("百分比必须在 0~1 之间");

        Value = value;
    }

    public static Percentage FromPercent(int percent) => new(percent / 100m);

    public int ToPercent() => (int)(Value * 100);

    public override string ToString() => $"{Value:P0}";
}
