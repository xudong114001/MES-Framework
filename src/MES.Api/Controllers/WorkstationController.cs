using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Domain.Entities;
using MES.Infrastructure.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/workstations")]
[Authorize]
public class WorkstationController : ControllerBase
{
    private readonly IRepository<Workstation> _repo;
    public WorkstationController(IRepository<Workstation> repo) => _repo = repo;

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
        if (entity == null) return NotFound(ApiResponse.Fail("工位不存在"));
        return Ok(ApiResponse.Ok(entity));
    }

    [HttpGet("by-line/{lineId}")]
    public async Task<IActionResult> GetByLine(long lineId)
    {
        var list = await _repo.FindAsync(ws => ws.LineId == lineId);
        return Ok(ApiResponse.Ok(list));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Workstation entity)
    {
        var created = await _repo.AddAsync(entity);
        return Ok(ApiResponse.Ok(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Workstation entity)
    {
        entity.Id = id;
        await _repo.UpdateAsync(entity);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse.Fail("工位不存在"));
        await _repo.DeleteAsync(entity);
        return Ok(ApiResponse.Ok("删除成功"));
    }
}
