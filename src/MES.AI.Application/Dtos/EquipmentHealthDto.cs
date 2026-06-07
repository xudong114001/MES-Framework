namespace MES.AI.Application.Dtos;

public class EquipmentHealthDto
{
    public long EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public int HealthScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public double Oee7DayTrend { get; set; }
    public double Oee30DayTrend { get; set; }
    public DateTime? PredictedMaintenanceDate { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}
