using MES.AI.Domain.Enums;
using MES.Domain.Entities;

namespace MES.AI.Domain.Entities;

public class KnowledgeEntry : BaseEntity
{
    public int Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Keywords { get; set; }
    public long? MaterialId { get; set; }
    public long? EquipmentId { get; set; }
}
