using MES.Domain.Exceptions;

namespace MES.Domain.Services;

/// <summary>
/// 设备领域服务 - 封装纯领域的设备效能计算逻辑
/// </summary>
public class EquipmentDomainService
{
    /// <summary>
    /// OEE 计算 - 纯领域公式
    /// OEE = 可用率(Availability) × 性能率(Performance) × 良品率(Quality)
    /// </summary>
    /// <param name="availability">可用率 (0~1)</param>
    /// <param name="performance">性能率 (0~1)</param>
    /// <param name="quality">良品率 (0~1)</param>
    /// <returns>OEE 值 (0~1)</returns>
    /// <exception cref="DomainException">当输入参数不在有效范围内时抛出</exception>
    public decimal CalculateOee(decimal availability, decimal performance, decimal quality)
    {
        ValidateRate(availability, nameof(availability));
        ValidateRate(performance, nameof(performance));
        ValidateRate(quality, nameof(quality));

        return availability * performance * quality;
    }

    /// <summary>
    /// 计算可用率
    /// Availability = 实际运行时间 / 计划运行时间
    /// </summary>
    /// <param name="actualRunTime">实际运行时间（小时）</param>
    /// <param name="plannedRunTime">计划运行时间（小时）</param>
    /// <returns>可用率 (0~1)</returns>
    public decimal CalculateAvailability(decimal actualRunTime, decimal plannedRunTime)
    {
        if (plannedRunTime <= 0)
            throw new DomainException("计划运行时间必须大于0");

        if (actualRunTime < 0)
            throw new DomainException("实际运行时间不能为负数");

        return Math.Min(actualRunTime / plannedRunTime, 1m);
    }

    /// <summary>
    /// 计算性能率
    /// Performance = 理论节拍 × 产出数量 / 实际运行时间
    /// </summary>
    /// <param name="theoreticalCycleTime">理论节拍（秒/件）</param>
    /// <param name="outputCount">产出数量</param>
    /// <param name="actualRunTimeSeconds">实际运行时间（秒）</param>
    /// <returns>性能率 (0~1)</returns>
    public decimal CalculatePerformance(decimal theoreticalCycleTime, decimal outputCount, decimal actualRunTimeSeconds)
    {
        if (theoreticalCycleTime <= 0)
            throw new DomainException("理论节拍必须大于0");

        if (outputCount < 0)
            throw new DomainException("产出数量不能为负数");

        if (actualRunTimeSeconds <= 0)
            throw new DomainException("实际运行时间必须大于0");

        return Math.Min((theoreticalCycleTime * outputCount) / actualRunTimeSeconds, 1m);
    }

    /// <summary>
    /// 计算良品率
    /// Quality = 良品数量 / 总产出数量
    /// </summary>
    /// <param name="goodCount">良品数量</param>
    /// <param name="totalCount">总产出数量（良品 + 不良品）</param>
    /// <returns>良品率 (0~1)</returns>
    public decimal CalculateQuality(decimal goodCount, decimal totalCount)
    {
        if (totalCount <= 0)
            throw new DomainException("总产出数量必须大于0");

        if (goodCount < 0)
            throw new DomainException("良品数量不能为负数");

        if (goodCount > totalCount)
            throw new DomainException("良品数量不能超过总产出数量");

        return goodCount / totalCount;
    }

    private static void ValidateRate(decimal value, string paramName)
    {
        if (value < 0 || value > 1)
            throw new DomainException($"{paramName} 必须在 0~1 之间，当前值: {value}");
    }
}
