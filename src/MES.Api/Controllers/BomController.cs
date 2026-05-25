using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Domain.Entities;
using MES.Infrastructure.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/boms")]
[Authorize]
public class BomController : ControllerBase
{
    private readonly IRepository<Bom> _repo;

    public BomController(IRepository<Bom> repo) => _repo = repo;

    /// <summary>
    /// 获取所有 BOM 明细
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _repo.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 根据 ID 获取 BOM 明细
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return NotFound(ApiResponse.Fail("BOM明细不存在"));
        return Ok(ApiResponse.Ok(entity));
    }

    /// <summary>
    /// 根据产品 ID 获取 BOM 列表
    /// </summary>
    [HttpGet("by-product/{productId}")]
    public async Task<IActionResult> GetByProductId(long productId)
    {
        var list = await _repo.FindAsync(b => b.ProductId == productId);
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 创建 BOM 明细
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Bom entity)
    {
        var created = await _repo.AddAsync(entity);
        return Ok(ApiResponse.Ok(created));
    }

    /// <summary>
    /// 更新 BOM 明细
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Bom entity)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse.Fail("BOM明细不存在"));

        entity.Id = id;
        await _repo.UpdateAsync(entity);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// 删除 BOM 明细
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return NotFound(ApiResponse.Fail("BOM明细不存在"));
        await _repo.DeleteAsync(entity);
        return Ok(ApiResponse.Ok("删除成功"));
    }
}
