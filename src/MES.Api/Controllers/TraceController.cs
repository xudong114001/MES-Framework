using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Interfaces;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/trace")]
[Authorize(Roles = "Admin,ProductionManager,QualityEngineer,EquipmentEngineer,Operator")]
public class TraceController : ControllerBase
{
    private readonly ITraceService _service;
    public TraceController(ITraceService service) => _service = service;

    /// <summary>
    /// 根据批次号追溯
    /// </summary>
    [HttpGet("by-batch/{batchNo}")]
    public async Task<IActionResult> ByBatch(string batchNo)
    {
        var result = await _service.TraceByBatchAsync(batchNo);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 根据序列号追溯
    /// </summary>
    [HttpGet("by-serial/{serialNo}")]
    public async Task<IActionResult> BySerial(string serialNo)
    {
        var result = await _service.TraceBySerialAsync(serialNo);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 正向追溯（物料用到哪些工单）
    /// </summary>
    [HttpGet("forward/{materialId}/{batchNo}")]
    public async Task<IActionResult> Forward(long materialId, string batchNo)
    {
        var result = await _service.TraceForwardAsync(materialId, batchNo);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 反向追溯（产品来自哪些物料）
    /// </summary>
    [HttpGet("backward/{serialNo}")]
    public async Task<IActionResult> Backward(string serialNo)
    {
        var result = await _service.TraceBackwardAsync(serialNo);
        return Ok(ApiResponse.Ok(result));
    }
}
