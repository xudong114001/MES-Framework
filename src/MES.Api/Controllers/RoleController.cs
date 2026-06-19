using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MES.Api.Middleware;
using MES.Domain.Entities;
using MES.Infrastructure.Data;
using MES.Infrastructure.Repositories;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "admin")]
public class RoleController : ControllerBase
{
    private readonly MesDbContext _db;
    private readonly IRepository<Role> _roleRepo;

    public RoleController(MesDbContext db, IRepository<Role> roleRepo)
    {
        _db = db;
        _roleRepo = roleRepo;
    }

    /// <summary>
    /// 获取所有角色列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Roles
            .Where(r => !r.IsDeleted)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                PermissionCount = _db.RolePermissions
                    .Where(rp => rp.RoleId == r.Id && !rp.IsDeleted)
                    .Count()
            })
            .ToListAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 获取角色详情（包括权限列表）
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var role = await _db.Roles
            .Where(r => r.Id == id && !r.IsDeleted)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                Permissions = _db.RolePermissions
                    .Where(rp => rp.RoleId == r.Id && !rp.IsDeleted)
                    .Select(rp => rp.Permission)
                    .ToList()
            })
            .FirstOrDefaultAsync();
        if (role == null)
            return NotFound(ApiResponse.Fail("角色不存在"));
        return Ok(ApiResponse.Ok(role));
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        if (await _db.Roles.AnyAsync(r => r.Name == request.Name && !r.IsDeleted))
            return BadRequest(ApiResponse.Fail("角色名称已存在"));

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description
        };

        await _roleRepo.AddAsync(role);
        return Ok(ApiResponse.Ok(role));
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRoleRequest request)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role == null || role.IsDeleted)
            return NotFound(ApiResponse.Fail("角色不存在"));

        if (request.Name != role.Name &&
            await _db.Roles.AnyAsync(r => r.Name == request.Name && !r.IsDeleted && r.Id != id))
            return BadRequest(ApiResponse.Fail("角色名称已存在"));

        role.Name = request.Name;
        role.Description = request.Description;

        await _roleRepo.UpdateAsync(role);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// 删除角色（软删除）
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role == null || role.IsDeleted)
            return NotFound(ApiResponse.Fail("角色不存在"));

        // 检查是否有关联用户
        var hasUsers = await _db.UserRoles.AnyAsync(ur => ur.RoleId == id && !ur.IsDeleted);
        if (hasUsers)
            return BadRequest(ApiResponse.Fail("该角色已分配给用户，无法删除"));

        await _roleRepo.DeleteAsync(role);
        return Ok(ApiResponse.Ok("删除成功"));
    }

    /// <summary>
    /// 获取角色权限列表
    /// </summary>
    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetPermissions(long id)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (role == null)
            return NotFound(ApiResponse.Fail("角色不存在"));

        var permissions = await _db.RolePermissions
            .Where(rp => rp.RoleId == id && !rp.IsDeleted)
            .Select(rp => rp.Permission)
            .ToListAsync();

        return Ok(ApiResponse.Ok(permissions));
    }

    /// <summary>
    /// 分配角色权限
    /// </summary>
    [HttpPut("{id}/permissions")]
    public async Task<IActionResult> AssignPermissions(long id, [FromBody] AssignPermissionsRequest request)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (role == null)
            return NotFound(ApiResponse.Fail("角色不存在"));

        // 删除现有权限
        var existingPerms = await _db.RolePermissions
            .Where(rp => rp.RoleId == id && !rp.IsDeleted)
            .ToListAsync();
        _db.RolePermissions.RemoveRange(existingPerms);

        // 添加新权限
        foreach (var perm in request.Permissions.Distinct())
        {
            _db.RolePermissions.Add(new RolePermission
            {
                RoleId = id,
                Permission = perm.Trim()
            });
        }

        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("权限分配成功"));
    }
}

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AssignPermissionsRequest
{
    public List<string> Permissions { get; set; } = [];
}