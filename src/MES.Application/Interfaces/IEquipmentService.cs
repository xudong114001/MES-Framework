using MES.Application.Dtos;
using MES.Domain.Entities;
using MES.Domain.Enums;

namespace MES.Application.Interfaces;

public interface IEquipmentService
{
    /// <summary>获取所有设备</summary>
    Task<IEnumerable<EquipmentDto>> GetAllAsync();

    /// <summary>根据ID获取设备</summary>
    Task<EquipmentDto?> GetByIdAsync(long id);

    /// <summary>创建设备</summary>
    Task<EquipmentDto> CreateAsync(EquipmentDto dto);

    /// <summary>更新设备</summary>
    Task UpdateAsync(long id, EquipmentDto dto);

    /// <summary>删除设备</summary>
    Task DeleteAsync(long id);

    /// <summary>记录保养</summary>
    Task RecordMaintenanceAsync(long equipmentId);

    /// <summary>报修</summary>
    Task ReportFaultAsync(long equipmentId);

    /// <summary>计算 OEE</summary>
    Task<OeeResult> CalculateOeeAsync(long equipmentId);

    /// <summary>创建保养计划</summary>
    Task<MaintenancePlan> CreateMaintenancePlanAsync(long equipmentId, string planName, int cycleDays, string? description);

    /// <summary>获取保养计划列表</summary>
    Task<List<MaintenancePlan>> GetMaintenancePlansAsync(long equipmentId);

    /// <summary>��成保养</summary>
    Task CompleteMaintenanceAsync(long planId);

    /// <summary>获取所有保养计划（可筛选）</summary>
    Task<List<MaintenancePlan>> GetAllMaintenancePlansAsync(string? equipmentName = null, MaintenancePlanStatus? status = null);

    /// <summary>获取所有设备（下拉列表用）</summary>
    Task<List<Equipment>> GetAllEquipmentAsync();

    /// <summary>更新保养计划</summary>
    Task<MaintenancePlan> UpdateMaintenancePlanAsync(long planId, string planName, int cycleDays, string? description);

    /// <summary>删除保养计划</summary>
    Task DeleteMaintenancePlanAsync(long planId);
}
