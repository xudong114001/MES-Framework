using MES.Domain.Exceptions;

namespace MES.Domain.ValueObjects;

/// <summary>
/// OEE 计算结果值对象
/// </summary>
public record OeeResult
{
    /// <summary>
    /// 可用率 (0~1)
    /// </summary>
    public decimal Availability { get; }

    /// <summary>
    /// 性能率 (0~1)
    /// </summary>
    public decimal Performance { get; }

    /// <summary>
    /// 良品率 (0~1)
    /// </summary>
    public decimal Quality { get; }

    /// <summary>
    /// OEE 综合值 (0~1)
    /// </summary>
    public decimal Oee { get; }

    public OeeResult(decimal availability, decimal performance, decimal quality)
    {
        ValidateRate(availability, nameof(availability));
        ValidateRate(performance, nameof(performance));
        ValidateRate(quality, nameof(quality));

        Availability = availability;
        Performance = performance;
        Quality = quality;
        Oee = availability * performance * quality;
    }

    private static void ValidateRate(decimal value, string paramName)
    {
        if (value < 0 || value > 1)
            throw new DomainException($"{paramName} 必须在 0~1 之间，当前值: {value}");
    }

    public override string ToString() =>
        $"OEE={Oee:P1} (A={Availability:P1}, P={Performance:P1}, Q={Quality:P1})";
}
