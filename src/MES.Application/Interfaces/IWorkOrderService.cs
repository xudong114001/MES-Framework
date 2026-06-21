using MES.Application.Dtos;
using MES.Domain.Entities;
using MES.Domain.Enums;

namespace MES.Application.Interfaces;

public interface IWorkOrderService
{
    Task<IEnumerable<WorkOrderDto>> GetAllAsync();
    Task<WorkOrderDto?> GetByIdAsync(long id);
    Task<WorkOrder> CreateWorkOrderAsync(WorkOrder workOrder);
    Task UpdateWorkOrderAsync(WorkOrder workOrder);
    Task DeleteWorkOrderAsync(long id);
    Task ReleaseWorkOrderAsync(long workOrderId);
    Task HoldWorkOrderAsync(long workOrderId);
    Task ResumeWorkOrderAsync(long workOrderId);
    Task CancelWorkOrderAsync(long workOrderId);
    Task CloseWorkOrderAsync(long workOrderId);
    Task<WorkOrder> SplitWorkOrderAsync(long workOrderId, decimal splitQty);
    Task<WorkOrder> ReworkWorkOrderAsync(long workOrderId, decimal reworkQty, string? remark);
    Task ScrapWorkOrderAsync(long workOrderId, decimal scrapQty, string? remark);
}
