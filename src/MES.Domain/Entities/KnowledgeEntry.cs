using MES.Domain.Enums;

namespace MES.Domain.Entities;

public class KnowledgeEntry : BaseEntity
{
    public int Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Keywords { get; set; }
    public long? MaterialId { get; set; }
    public long? EquipmentId { get; set; }
}
