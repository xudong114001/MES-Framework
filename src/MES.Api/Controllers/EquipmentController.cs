using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Dtos;
using MES.Application.Interfaces;
using MES.Domain.Enums;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/equipment")]
[Authorize(Roles = "admin,supervisor")]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;

    public EquipmentController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    /// <summary>
    /// 获取所有设备
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _equipmentService.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 获取设备列表（下拉用）
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetEquipmentList()
    {
        var list = await _equipmentService.GetAllEquipmentAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 根据 ID 获取设备
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _equipmentService.GetByIdAsync(id);
        if (entity == null)
            return NotFound(ApiResponse.Fail("设备不存在"));
        return Ok(ApiResponse.Ok(entity));
    }

    /// <summary>
    /// 创建设备
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EquipmentDto dto)
    {
        var created = await _equipmentService.CreateAsync(dto);
        return Ok(ApiResponse.Ok(created));
    }

    /// <summary>
    /// 更新设备
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] EquipmentDto dto)
    {
        await _equipmentService.UpdateAsync(id, dto);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// 删除设备
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _equipmentService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("删除成功"));
    }

    /// <summary>
    /// 记录保养
    /// </summary>
    [HttpPost("{id}/maintain")]
    public async Task<IActionResult> Maintain(long id)
    {
        await _equipmentService.RecordMaintenanceAsync(id);
        return Ok(ApiResponse.Ok("保养记录成功"));
    }

    /// <summary>
    /// 报修
    /// </summary>
    [HttpPost("{id}/fault")]
    public async Task<IActionResult> Fault(long id)
    {
        await _equipmentService.ReportFaultAsync(id);
        return Ok(ApiResponse.Ok("报修已提交"));
    }

    /// <summary>
    /// OEE 数据（真实计算）
    /// </summary>
    [HttpGet("{id}/oee")]
    public async Task<IActionResult> Oee(long id)
    {
        var result = await _equipmentService.CalculateOeeAsync(id);
        return Ok(ApiResponse.Ok(result));
    }

    // ======================== 保养计划 ========================

    /// <summary>
    /// 创建保养计划
    /// </summary>
    [HttpPost("{id}/maintenance-plan")]
    public async Task<IActionResult> CreateMaintenancePlan(long id, [FromBody] CreateMaintenancePlanRequest request)
    {
        var result = await _equipmentService.CreateMaintenancePlanAsync(id, request.PlanName, request.CycleDays, request.Description);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 获取保养计划列表
    /// </summary>
    [HttpGet("{id}/maintenance-plans")]
    public async Task<IActionResult> GetMaintenancePlans(long id)
    {
        var result = await _equipmentService.GetMaintenancePlansAsync(id);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 完成保养
    /// </summary>
    [HttpPost("{id}/maintenance-plan/{planId}/complete")]
    public async Task<IActionResult> CompleteMaintenance(long id, long planId)
    {
        await _equipmentService.CompleteMaintenanceAsync(planId);
        return Ok(ApiResponse.Ok("保养完成"));
    }

    /// <summary>
    /// 获取所有保养计划（可筛选）
    /// </summary>
    [HttpGet("maintenance-plans")]
    public async Task<IActionResult> GetAllMaintenancePlans(
        [FromQuery] string? equipmentName,
        [FromQuery] MaintenancePlanStatus? status)
    {
        var result = await _equipmentService.GetAllMaintenancePlansAsync(equipmentName, status);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 更新保养计划
    /// </summary>
    [HttpPut("maintenance-plans/{id}")]
    public async Task<IActionResult> UpdateMaintenancePlan(long id, [FromBody] UpdateMaintenancePlanRequest request)
    {
        var result = await _equipmentService.UpdateMaintenancePlanAsync(id, request.PlanName, request.CycleDays, request.Description);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 删除保养计划
    /// </summary>
    [HttpDelete("maintenance-plans/{id}")]
    public async Task<IActionResult> DeleteMaintenancePlan(long id)
    {
        await _equipmentService.DeleteMaintenancePlanAsync(id);
        return Ok(ApiResponse.Ok("删除成功"));
    }
}

public class CreateMaintenancePlanRequest
{
    public string PlanName { get; set; } = string.Empty;
    public int CycleDays { get; set; }
    public string? Description { get; set; }
}

public class UpdateMaintenancePlanRequest
{
    public string PlanName { get; set; } = string.Empty;
    public int CycleDays { get; set; }
    public string? Description { get; set; }
}
