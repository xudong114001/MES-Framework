using MES.Domain.AggregateRoots;
using MES.Domain.Exceptions;
using System.Linq;

namespace MES.Domain.Entities;

public class Routing : BaseEntity, IAggregateRoot
{
    private readonly List<RoutingStep> _steps = new();

    public long MaterialId { get; internal set; }
    public string RoutingCode { get; internal set; } = string.Empty;
    public string RoutingName { get; internal set; } = string.Empty;
    public string Version { get; internal set; } = "V1.0";
    public bool Status { get; internal set; } = true;

    public virtual Material? Material { get; set; }
    public IReadOnlyCollection<RoutingStep> Steps => _steps.AsReadOnly();

    /// <summary>
    /// EF Core 需要的无参构造函数（内部使用）
    /// </summary>
    internal Routing() { }

    #region 工厂方法

    /// <summary>
    /// 创建工艺路线
    /// </summary>
    public static Routing Create(
        long materialId,
        string routingCode,
        string routingName,
        string version = "V1.0",
        bool status = true)
    {
        if (string.IsNullOrWhiteSpace(routingCode))
            throw new DomainException("工艺路线编码不能为空");

        if (string.IsNullOrWhiteSpace(routingName))
            throw new DomainException("工艺路线名称不能为空");

        if (materialId <= 0)
            throw new DomainException("物料ID无效");

        return new Routing
        {
            MaterialId = materialId,
            RoutingCode = routingCode,
            RoutingName = routingName,
            Version = version,
            Status = status
        };
    }

    #endregion

    #region 行为方法

    /// <summary>
    /// 添加工序
    /// </summary>
    public void AddStep(RoutingStep step)
    {
        if (step == null)
            throw new DomainException("工序不能为空");

        step.RoutingId = Id;
        _steps.Add(step);
    }

    /// <summary>
    /// 重新排序工序
    /// </summary>
    public void ReorderSteps(IEnumerable<int> newOrder)
    {
        var orderList = newOrder.ToList();

        if (orderList.Count != _steps.Count)
            throw new DomainException("排序数量与现有工序数量不匹配");

        // 验证所有ID都存在且没有重复
        var currentIds = _steps.Select(s => (int)s.Id).OrderBy(id => id).ToList();
        var newIdsSorted = orderList.Distinct().OrderBy(id => id).ToList();

        // 手动比较两个列表
        if (currentIds.Count != newIdsSorted.Count ||
            !currentIds.Zip(newIdsSorted, (a, b) => a == b).All(x => x))
            throw new DomainException("排序ID列表与现有工序ID不匹配");

        // 更新序号
        for (int i = 0; i < orderList.Count; i++)
        {
            var step = _steps.First(s => s.Id == orderList[i]);
            step.SetStepNo(i + 1);
        }
    }

    /// <summary>
    /// 启用/禁用工艺路线
    /// </summary>
    public void SetStatus(bool isActive)
    {
        Status = isActive;
    }

    /// <summary>
    /// 更新版本
    /// </summary>
    public void UpdateVersion(string newVersion)
    {
        if (string.IsNullOrWhiteSpace(newVersion))
            throw new DomainException("版本号不能为空");

        Version = newVersion;
    }

    /// <summary>
    /// 更新工艺路线信息
    /// </summary>
    public void UpdateInfo(long materialId, string routingCode, string routingName, string version, bool status)
    {
        if (string.IsNullOrWhiteSpace(routingCode))
            throw new DomainException("工艺路线编码不能为空");

        if (string.IsNullOrWhiteSpace(routingName))
            throw new DomainException("工艺路线名称不能为空");

        if (materialId <= 0)
            throw new DomainException("物料ID无效");

        MaterialId = materialId;
        RoutingCode = routingCode;
        RoutingName = routingName;
        Version = version;
        Status = status;
    }

    #endregion
}
