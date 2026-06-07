namespace MES.Integration.Dtos;

public class SyncWorkReportDto
{
    public string ReportNo { get; set; } = string.Empty;
    public string OrderNo { get; set; } = string.Empty;
    public string? StepCode { get; set; }
    public string? WorkstationCode { get; set; }
    public string? OperatorCode { get; set; }
    public decimal GoodQty { get; set; }
    public decimal ScrapQty { get; set; }
    public decimal ReworkQty { get; set; }
    public int DurationMin { get; set; }
    public DateTime ReportTime { get; set; }
    public string? BatchNo { get; set; }
    public string? Remark { get; set; }
}
