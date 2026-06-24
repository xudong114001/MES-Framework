namespace MES.Application.Dtos;

/// <summary>
/// 工艺路线详情 DTO（含工序列表）
/// </summary>
public class RoutingDetailDto
{
    public RoutingDto Dto { get; set; } = null!;
    public IEnumerable<RoutingStepDto> Steps { get; set; } = [];
}
