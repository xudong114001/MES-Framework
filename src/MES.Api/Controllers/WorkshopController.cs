using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Domain.Entities;
using MES.Infrastructure.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/workshops")]
[Authorize(Roles = "admin,supervisor")]
public class WorkshopController : ControllerBase
{
    private readonly IRepository<Workshop> _repo;
    public WorkshopController(IRepository<Workshop> repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _repo.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse.Fail("车间不存在"));
        return Ok(ApiResponse.Ok(entity));
    }

    [HttpGet("by-factory/{factoryId}")]
    public async Task<IActionResult> GetByFactory(long factoryId)
    {
        var list = await _repo.FindAsync(w => w.FactoryId == factoryId);
        return Ok(ApiResponse.Ok(list));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Workshop entity)
    {
        var created = await _repo.AddAsync(entity);
        return Ok(ApiResponse.Ok(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Workshop entity)
    {
        entity.Id = id;
        await _repo.UpdateAsync(entity);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse.Fail("车间不存在"));
        await _repo.DeleteAsync(entity);
        return Ok(ApiResponse.Ok("删除成功"));
    }
}
