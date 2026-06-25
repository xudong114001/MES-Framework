using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Dtos;
using MES.Application.Interfaces;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/production-lines")]
[Authorize(Roles = "admin,supervisor")]
public class ProductionLineController : ControllerBase
{
    private readonly IProductionLineService _service;
    public ProductionLineController(IProductionLineService service) => _service = service;

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
        if (dto == null) return NotFound(ApiResponse.Fail("产线不存在"));
        return Ok(ApiResponse.Ok(dto));
    }

    [HttpGet("by-workshop/{workshopId}")]
    public async Task<IActionResult> GetByWorkshop(long workshopId)
    {
        var list = await _service.GetByWorkshopIdAsync(workshopId);
        return Ok(ApiResponse.Ok(list));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionLineRequest request)
    {
        var created = await _service.CreateAsync(request);
        return Ok(ApiResponse.Ok(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductionLineRequest request)
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
