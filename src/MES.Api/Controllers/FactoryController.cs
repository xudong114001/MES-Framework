using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Interfaces;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/factories")]
[Authorize(Roles = "admin,supervisor")]
public class FactoryController : ControllerBase
{
    private readonly IFactoryService _service;
    public FactoryController(IFactoryService service) => _service = service;

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
        if (dto == null) return NotFound(ApiResponse.Fail("工厂不存在"));
        return Ok(ApiResponse.Ok(dto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MES.Domain.Entities.Factory entity)
    {
        var created = await _service.CreateAsync(entity);
        return Ok(ApiResponse.Ok(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] MES.Domain.Entities.Factory entity)
    {
        await _service.UpdateAsync(id, entity);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("删除成功"));
    }
}
