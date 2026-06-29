namespace MES.Application.Dtos;

public class TodayOrderStatsDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
}

public class OrderStatusDistributionDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class OutputStatsDto
{
    public decimal GoodQty { get; set; }
    public decimal ScrapQty { get; set; }
    public decimal ReworkQty { get; set; }
    public int WorkOrderCount { get; set; }
}

public class EquipmentStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}
