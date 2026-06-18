using MES.AI.Domain.Enums;

namespace MES.AI.Application.Dtos;

public class KnowledgeEntryDto
{
    public long Id { get; set; }
    public int Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Keywords { get; set; }
    public long? MaterialId { get; set; }
    public long? EquipmentId { get; set; }
    public string CategoryName => Category switch
    {
        (int)KnowledgeCategory.ProcessStandard => "工艺标准",
        (int)KnowledgeCategory.QualitySpec => "质检规范",
        (int)KnowledgeCategory.EquipmentManual => "设备手册",
        (int)KnowledgeCategory.SafetyRegulation => "安全规程",
        (int)KnowledgeCategory.General => "通用",
        _ => "未知"
    };
}
