using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Domain.Entities;
using MES.Infrastructure.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/workstations")]
[Authorize(Roles = "admin,supervisor")]
public class WorkstationController : ControllerBase
{
    private readonly IRepository<Workstation> _repo;
    public WorkstationController(IRepository<Workstation> repo) => _repo = repo;

    /// <summary>
    /// 获取所有工位
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _repo.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 根据ID获取工位
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse.Fail("工位不存在"));
        return Ok(ApiResponse.Ok(entity));
    }

    /// <summary>
    /// 根据产线ID获取工位列表
    /// </summary>
    [HttpGet("by-line/{lineId}")]
    public async Task<IActionResult> GetByLine(long lineId)
    {
        var list = await _repo.FindAsync(ws => ws.LineId == lineId);
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 创建工位
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Workstation entity)
    {
        var created = await _repo.AddAsync(entity);
        return Ok(ApiResponse.Ok(created));
    }

    /// <summary>
    /// 更新工位
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Workstation entity)
    {
        entity.Id = id;
        await _repo.UpdateAsync(entity);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// 删除工位
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse.Fail("工位不存在"));
        await _repo.DeleteAsync(entity);
        return Ok(ApiResponse.Ok("删除成功"));
    }
}
