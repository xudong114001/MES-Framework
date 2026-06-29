namespace MES.Application.Dtos;

public class ScheduleRequest
{
    public long WorkOrderId { get; set; }
    public long LineId { get; set; }
}

public class BatchScheduleRequest
{
    public List<long> WorkOrderIds { get; set; } = new();
    public long LineId { get; set; }
}

public class SwapOrderRequest
{
    public long OrderId1 { get; set; }
    public long OrderId2 { get; set; }
}
