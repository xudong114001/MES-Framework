using MES.Domain.Enums;

namespace MES.Application.Dtos;

public class QcInspectionItemDto
{
    public long Id { get; set; }
    public long InspectionId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? SpecValue { get; set; }
    public string? ActualValue { get; set; }
    public QcResult Result { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}

public record InspectionWithItemsDto(QcInspectionDto Inspection, IEnumerable<QcInspectionItemDto> Items);
