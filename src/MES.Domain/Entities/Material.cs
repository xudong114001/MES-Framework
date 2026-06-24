using MES.Domain.AggregateRoots;
using MES.Domain.Exceptions;

namespace MES.Domain.Entities;

public class Material : BaseEntity, IAggregateRoot
{
    internal Material() { }

    public static Material Create(
        string code,
        string name,
        string? spec = null,
        string? unit = null,
        string? category = null,
        int? bomLevel = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("物料编码不能为空", "MATERIAL_CODE_REQUIRED");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("物料名称不能为空", "MATERIAL_NAME_REQUIRED");

        return new Material
        {
            Code = code,
            Name = name,
            Spec = spec,
            Unit = unit,
            Category = category,
            BomLevel = bomLevel,
            StockQty = 0,
            Status = true
        };
    }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Spec { get; set; }
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public int? BomLevel { get; set; }
    public decimal StockQty { get; set; }
    public bool Status { get; set; } = true;

    /// <summary>
    /// 库存调整
    /// </summary>
    /// <param name="delta">变化量（正数入库，负数出库）</param>
    /// <param name="reason">调整原因</param>
    public void AdjustStock(decimal delta, string reason)
    {
        if (!Status)
            throw new DomainException("已停用的物料不允许调整库存", "MATERIAL_INACTIVE");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("库存调整必须填写原因", "ADJUST_REASON_REQUIRED");

        var newQty = StockQty + delta;
        if (newQty < 0)
            throw new DomainException($"库存不足：当前库存 {StockQty}，调整量 {delta}", "STOCK_INSUFFICIENT");

        StockQty = newQty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 更新规格信息
    /// </summary>
    public void UpdateSpec(string? spec, string? unit, string? category)
    {
        if (!Status)
            throw new DomainException("已停用的物料不允许更新规格", "MATERIAL_INACTIVE");

        Spec = spec;
        Unit = unit;
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 停用物料
    /// </summary>
    public void Deactivate()
    {
        if (!Status)
            throw new DomainException("物料已经处于停用状态", "MATERIAL_ALREADY_INACTIVE");

        Status = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 启用物料
    /// </summary>
    public void Activate()
    {
        if (Status)
            throw new DomainException("物料已经处于启用状态", "MATERIAL_ALREADY_ACTIVE");

        Status = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
