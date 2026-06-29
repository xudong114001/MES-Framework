namespace MES.Application.Dtos;

public class TraceResultDto
{
    public string TraceType { get; set; } = string.Empty;
    public string BatchNo { get; set; } = string.Empty;
    public string? SerialNo { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public List<TraceStepDto> Steps { get; set; } = new();
}

public class TraceStepDto
{
    public long WorkOrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public DateTime OperateTime { get; set; }
}
