using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MES.Api.Middleware;
using MES.Application.Dtos;
using MES.Domain.Entities;
using MES.Infrastructure.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/routings")]
[Authorize(Roles = "admin,supervisor")]
public class RoutingController : ControllerBase
{
    private readonly IRepository<Routing> _repo;

    public RoutingController(IRepository<Routing> repo) => _repo = repo;

    private static RoutingDto MapToDto(Routing entity) => new()
    {
        Id = entity.Id,
        MaterialId = entity.MaterialId,
        RoutingCode = entity.RoutingCode,
        RoutingName = entity.RoutingName,
        Version = entity.Version,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };

    private static RoutingStepDto MapStepToDto(RoutingStep entity) => new()
    {
        Id = entity.Id,
        RoutingId = entity.RoutingId,
        StepNo = entity.StepNo,
        StepName = entity.StepName,
        WorkstationType = entity.WorkstationType,
        StandardTime = entity.StandardTime,
        IsQcPoint = entity.IsQcPoint,
        IsScrapPoint = entity.IsScrapPoint,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };

    /// <summary>
    /// 获取所有工艺路线（包含工序明细）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _repo.Query().Include(r => r.Steps).ToListAsync();
        var result = list.Select(r => new
        {
            Dto = MapToDto(r),
            Steps = r.Steps.Select(MapStepToDto).ToList()
        });
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 根据 ID 获取工艺路线（包含工序明细）
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var entity = await _repo.Query().Include(r => r.Steps).FirstOrDefaultAsync(r => r.Id == id);
        if (entity == null)
            return NotFound(ApiResponse.Fail("工艺路线不存在"));
        var result = new
        {
            Dto = MapToDto(entity),
            Steps = entity.Steps.Select(MapStepToDto).ToList()
        };
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 根据物料 ID 获取工艺路线（包含工序明细）
    /// </summary>
    [HttpGet("by-material/{materialId}")]
    public async Task<IActionResult> GetByMaterialId(long materialId)
    {
        var list = await _repo.Query()
            .Include(r => r.Steps)
            .Where(r => r.MaterialId == materialId)
            .ToListAsync();
        var result = list.Select(r => new
        {
            Dto = MapToDto(r),
            Steps = r.Steps.Select(MapStepToDto).ToList()
        });
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 创建工艺路线
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Routing entity)
    {
        var created = await _repo.AddAsync(entity);
        return Ok(ApiResponse.Ok(MapToDto(created)));
    }

    /// <summary>
    /// 更新工艺路线
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Routing entity)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return NotFound(ApiResponse.Fail("工艺路线不存在"));

        entity.Id = id;
        await _repo.UpdateAsync(entity);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// 删除工艺路线
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return NotFound(ApiResponse.Fail("工艺路线不存在"));
        await _repo.DeleteAsync(entity);
        return Ok(ApiResponse.Ok("删除成功"));
    }
}