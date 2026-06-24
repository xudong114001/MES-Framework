using MES.Domain.AggregateRoots;
using MES.Domain.Exceptions;
using MES.Domain.ValueObjects;

namespace MES.Domain.Entities;

public class Bom : BaseEntity, IAggregateRoot
{
    internal Bom() { }

    public static Bom Create(long productId, long materialId, Quantity quantity, decimal scrapRate, int seqNo)
    {
        if (productId <= 0)
            throw new DomainException("产品ID无效", "BOM_PRODUCT_ID_INVALID");
        if (materialId <= 0)
            throw new DomainException("物料ID无效", "BOM_MATERIAL_ID_INVALID");
        if (quantity.Value <= 0)
            throw new DomainException("用量必须大于0", "BOM_QUANTITY_INVALID");
        if (scrapRate < 0 || scrapRate > 1)
            throw new DomainException("损耗率必须在0-1之间", "BOM_SCRAP_RATE_INVALID");

        return new Bom
        {
            ProductId = productId,
            MaterialId = materialId,
            Quantity = quantity,
            ScrapRate = scrapRate,
            SeqNo = seqNo,
            ValidFrom = DateTime.UtcNow,
            Status = true
        };
    }

    public long ProductId { get; set; }
    public long MaterialId { get; set; }
    public Quantity Quantity { get; set; } = Quantity.Zero();
    public decimal ScrapRate { get; set; }
    public int SeqNo { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool Status { get; set; } = true;

    public virtual Material? Product { get; set; }
    public virtual Material? Material { get; set; }

    /// <summary>
    /// 更新用量和损耗率
    /// </summary>
    public void UpdateQuantity(Quantity quantity, decimal scrapRate)
    {
        if (quantity.Value <= 0)
            throw new DomainException("用量必须大于0", "BOM_QUANTITY_INVALID");
        if (scrapRate < 0 || scrapRate > 1)
            throw new DomainException("损耗率必须在0-1之间", "BOM_SCRAP_RATE_INVALID");

        Quantity = quantity;
        ScrapRate = scrapRate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 停用 BOM
    /// </summary>
    public void Deactivate()
    {
        if (!Status)
            throw new DomainException("BOM已经处于停用状态", "BOM_ALREADY_INACTIVE");

        Status = false;
        ValidTo = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 启用 BOM
    /// </summary>
    public void Activate()
    {
        if (Status)
            throw new DomainException("BOM已经处于启用状态", "BOM_ALREADY_ACTIVE");

        Status = true;
        ValidTo = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
