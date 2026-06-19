using System.Security.Cryptography;
using System.Text;
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
public class UserController : ControllerBase
{
    private readonly MesDbContext _db;
    private readonly IRepository<User> _repo;
    private readonly IRepository<Role> _roleRepo;

    public UserController(MesDbContext db, IRepository<User> repo, IRepository<Role> roleRepo)
    {
        _db = db;
        _repo = repo;
        _roleRepo = roleRepo;
    }

    /// <summary>
    /// 获取所有用户列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Users
            .Where(u => !u.IsDeleted)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.DisplayName,
                u.Email,
                u.Phone,
                u.Status,
                u.LastLoginTime,
                Roles = _db.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Select(ur => ur.Role!.Name)
                    .ToList()
            })
            .ToListAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 获取用户详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var user = await _db.Users
            .Where(u => u.Id == id && !u.IsDeleted)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.DisplayName,
                u.Email,
                u.Phone,
                u.Status,
                u.LastLoginTime,
                Roles = _db.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Select(ur => ur.Role!.Name)
                    .ToList()
            })
            .FirstOrDefaultAsync();
        if (user == null)
            return NotFound(ApiResponse.Fail("用户不存在"));
        return Ok(ApiResponse.Ok(user));
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Username == request.Username && !u.IsDeleted))
            return BadRequest(ApiResponse.Fail("用户名已存在"));

        var passwordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password)));

        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            DisplayName = request.DisplayName,
            Email = request.Email,
            Phone = request.Phone,
            Status = request.Status
        };

        await _repo.AddAsync(user);
        return Ok(ApiResponse.Ok(user));
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequest request)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
            return NotFound(ApiResponse.Fail("用户不存在"));

        if (request.Username != user.Username &&
            await _db.Users.AnyAsync(u => u.Username == request.Username && !u.IsDeleted && u.Id != id))
            return BadRequest(ApiResponse.Fail("用户名已存在"));

        user.Username = request.Username;
        user.DisplayName = request.DisplayName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.Status = request.Status;

        await _repo.UpdateAsync(user);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// 删除用户（软删除）
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
            return NotFound(ApiResponse.Fail("用户不存在"));

        if (user.Username == "admin")
            return BadRequest(ApiResponse.Fail("不能删除超级管理员"));

        await _repo.DeleteAsync(user);
        return Ok(ApiResponse.Ok("删除成功"));
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(long id, [FromBody] ResetPasswordRequest request)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
            return NotFound(ApiResponse.Fail("用户不存在"));

        user.PasswordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.NewPassword)));
        await _repo.UpdateAsync(user);
        return Ok(ApiResponse.Ok("密码已重置"));
    }

    /// <summary>
    /// 分配用户角色
    /// </summary>
    [HttpPut("{id}/roles")]
    public async Task<IActionResult> AssignRoles(long id, [FromBody] AssignRolesRequest request)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
            return NotFound(ApiResponse.Fail("用户不存在"));

        var existingRoles = await _db.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
        _db.UserRoles.RemoveRange(existingRoles);

        foreach (var roleName in request.Roles)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName && !r.IsDeleted);
            if (role != null)
            {
                _db.UserRoles.Add(new UserRole { UserId = id, RoleId = role.Id });
            }
        }

        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("角色分配成功"));
    }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool Status { get; set; } = true;
}

public class UpdateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool Status { get; set; } = true;
}

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

public class AssignRolesRequest
{
    public List<string> Roles { get; set; } = [];
}