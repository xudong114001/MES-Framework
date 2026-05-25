using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Services;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/trace")]
[Authorize]
public class TraceController : ControllerBase
{
    private readonly TraceService _service;
    public TraceController(TraceService service) => _service = service;

    [HttpGet("by-batch/{batchNo}")]
    public async Task<IActionResult> ByBatch(string batchNo)
    {
        var result = await _service.TraceByBatchAsync(batchNo);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("by-serial/{serialNo}")]
    public async Task<IActionResult> BySerial(string serialNo)
    {
        var result = await _service.TraceBySerialAsync(serialNo);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("forward/{materialId}/{batchNo}")]
    public async Task<IActionResult> Forward(long materialId, string batchNo)
    {
        var result = await _service.TraceForwardAsync(materialId, batchNo);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("backward/{serialNo}")]
    public async Task<IActionResult> Backward(string serialNo)
    {
        var result = await _service.TraceBackwardAsync(serialNo);
        return Ok(ApiResponse.Ok(result));
    }
}
