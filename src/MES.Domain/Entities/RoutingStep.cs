using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class RoutingStep : BaseEntity
{
    public long RoutingId { get; internal set; }
    public int StepNo { get; internal set; }
    public string StepName { get; private set; } = string.Empty;
    public string? WorkstationType { get; private set; }
    public decimal StandardTime { get; internal set; }
    public bool IsQcPoint { get; internal set; }
    public bool IsScrapPoint { get; internal set; }

    public virtual Routing? Routing { get; set; }

    /// <summary>
    /// EF Core 需要的无参构造函数（内部使用）
    /// </summary>
    internal RoutingStep() { }

    /// <summary>
    /// 创建工艺工序
    /// </summary>
    public static RoutingStep Create(
        long routingId,
        string stepName,
        int stepNo,
        decimal standardTime,
        string? workstationType = null,
        bool isQcPoint = false,
        bool isScrapPoint = false)
    {
        if (string.IsNullOrWhiteSpace(stepName))
            throw new DomainException("工序名称不能为空");

        if (stepNo <= 0)
            throw new DomainException("工序序号必须大于0");

        return new RoutingStep
        {
            RoutingId = routingId,
            StepNo = stepNo,
            StepName = stepName,
            WorkstationType = workstationType,
            StandardTime = standardTime,
            IsQcPoint = isQcPoint,
            IsScrapPoint = isScrapPoint
        };
    }

    /// <summary>
    /// 创建工艺工序
    /// </summary>
    public RoutingStep(
        int stepNo,
        string stepName,
        string? workstationType = null,
        decimal standardTime = 0,
        bool isQcPoint = false,
        bool isScrapPoint = false)
    {
        if (string.IsNullOrWhiteSpace(stepName))
            throw new DomainException("工序名称不能为空");

        if (stepNo <= 0)
            throw new DomainException("工序序号必须大于0");

        StepNo = stepNo;
        StepName = stepName;
        WorkstationType = workstationType;
        StandardTime = standardTime;
        IsQcPoint = isQcPoint;
        IsScrapPoint = isScrapPoint;
    }

    /// <summary>
    /// 设置工序序号（内部使用）
    /// </summary>
    public void SetStepNo(int stepNo)
    {
        if (stepNo <= 0)
            throw new DomainException("工序序号必须大于0");

        StepNo = stepNo;
    }

    /// <summary>
    /// 更新标准工时
    /// </summary>
    public void UpdateStandardTime(decimal standardTime)
    {
        if (standardTime < 0)
            throw new DomainException("标准工时不能为负数");

        StandardTime = standardTime;
    }

    /// <summary>
    /// 设置是否为质检点
    /// </summary>
    public void SetQcPoint(bool isQcPoint)
    {
        IsQcPoint = isQcPoint;
    }

    /// <summary>
    /// 设置是否为报废点
    /// </summary>
    public void SetScrapPoint(bool isScrapPoint)
    {
        IsScrapPoint = isScrapPoint;
    }
}