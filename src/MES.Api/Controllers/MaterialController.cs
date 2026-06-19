using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Domain.Entities;
using MES.Infrastructure.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/materials")]
[Authorize(Roles = "admin,supervisor")]
public class MaterialController : ControllerBase
{
    private readonly IRepository<Material> _repo;

    public MaterialController(IRepository<Material> repo) => _repo = repo;

    /// <summary>
    /// 获取所有物料
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _repo.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 根据 ID 获取物料
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return NotFound(ApiResponse.Fail("物料不存在"));
        return Ok(ApiResponse.Ok(entity));
    }

    /// <summary>
    /// 创建物料
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Material entity)
    {
        var created = await _repo.AddAsync(entity);
        return Ok(ApiResponse.Ok(created));
    }

    /// <summary>
    /// 更新物料
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Material entity)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse.Fail("物料不存在"));

        // 保留原有导航属性和审计字段
        entity.Id = id;
        entity.CreatedAt = existing.CreatedAt;
        entity.CreatedBy = existing.CreatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// 删除物料
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return NotFound(ApiResponse.Fail("物料不存在"));
        await _repo.DeleteAsync(entity);
        return Ok(ApiResponse.Ok("删除成功"));
    }
}
