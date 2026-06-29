using MES.Application.Dtos;

namespace MES.Application.Interfaces;

/// <summary>
/// 派工服务接口
/// </summary>
public interface IDispatchService
{
    /// <summary>将工单的某工序派工到指定工位</summary>
    Task DispatchStepAsync(long workOrderStepId, long workstationId);

    /// <summary>取消某工序的派工（清除工位绑定）</summary>
    Task UndispatchStepAsync(long workOrderStepId);

    /// <summary>按产线查询今日派工任务</summary>
    Task<IEnumerable<WorkOrderDto>> GetTodayDispatchedOrdersByLineAsync(long lineId);

    /// <summary>获取某工序可选工位（所属产线下的所有工位）</summary>
    Task<IEnumerable<WorkstationDto>> GetAvailableWorkstationsAsync(long lineId);
}
