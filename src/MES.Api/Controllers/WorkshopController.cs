using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Dtos;
using MES.Application.Interfaces;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/workshops")]
[Authorize(Roles = "Admin,ProductionManager")]
public class WorkshopController : ControllerBase
{
    private readonly IWorkshopService _service;
    public WorkshopController(IWorkshopService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto == null) return NotFound(ApiResponse.Fail("车间不存在"));
        return Ok(ApiResponse.Ok(dto));
    }

    [HttpGet("by-factory/{factoryId}")]
    public async Task<IActionResult> GetByFactory(long factoryId)
    {
        var list = await _service.GetByFactoryIdAsync(factoryId);
        return Ok(ApiResponse.Ok(list));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkshopRequest request)
    {
        var created = await _service.CreateAsync(request);
        return Ok(ApiResponse.Ok(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateWorkshopRequest request)
    {
        await _service.UpdateAsync(id, request);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("删除成功"));
    }
}
