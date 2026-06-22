namespace MES.Application.Dtos;

public class OeeResult
{
    public long EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double OeeValue { get; set; }
    public double Availability { get; set; }
    public double Performance { get; set; }
    public double Quality { get; set; }
    public double GoodQty { get; set; }
    public double BadQty { get; set; }
    public double ActualRunMinutes { get; set; }
    public double PlannedRunMinutes { get; set; }
    public DateTime? LastMaintainTime { get; set; }
    public DateTime? NextMaintainTime { get; set; }
    public int? MaintainCycleDays { get; set; }
    public double? TheoreticalCycleTime { get; set; }
    public double? PlannedRunTime { get; set; }
}
