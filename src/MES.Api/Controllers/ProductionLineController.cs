using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Domain.Entities;
using MES.Infrastructure.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/production-lines")]
[Authorize(Roles = "admin,supervisor")]
public class ProductionLineController : ControllerBase
{
    private readonly IRepository<ProductionLine> _repo;
    public ProductionLineController(IRepository<ProductionLine> repo) => _repo = repo;

    /// <summary>
    /// 获取所有产线
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _repo.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 根据ID获取产线
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse.Fail("产线不存在"));
        return Ok(ApiResponse.Ok(entity));
    }

    /// <summary>
    /// 根据车间ID获取产线列表
    /// </summary>
    [HttpGet("by-workshop/{workshopId}")]
    public async Task<IActionResult> GetByWorkshop(long workshopId)
    {
        var list = await _repo.FindAsync(pl => pl.WorkshopId == workshopId);
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 创建产线
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductionLine entity)
    {
        var created = await _repo.AddAsync(entity);
        return Ok(ApiResponse.Ok(created));
    }

    /// <summary>
    /// 更新产线
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] ProductionLine entity)
    {
        entity.Id = id;
        await _repo.UpdateAsync(entity);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// 删除产线
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse.Fail("产线不存在"));
        await _repo.DeleteAsync(entity);
        return Ok(ApiResponse.Ok("删除成功"));
    }
}
