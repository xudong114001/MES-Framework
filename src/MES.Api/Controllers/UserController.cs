using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Dtos;
using MES.Application.Interfaces;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 获取所有用户列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _userService.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    /// <summary>
    /// 获取用户详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var user = await _userService.GetByIdAsync(id);
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
        var created = await _userService.CreateAsync(request);
        return Ok(ApiResponse.Ok(created));
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequest request)
    {
        await _userService.UpdateAsync(id, request);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    /// <summary>
    /// 删除用户（软删除）
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _userService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("删除成功"));
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(long id, [FromBody] ResetPasswordRequest request)
    {
        await _userService.ResetPasswordAsync(id, request.NewPassword);
        return Ok(ApiResponse.Ok("密码已重置"));
    }

    /// <summary>
    /// 分配用户角色
    /// </summary>
    [HttpPut("{id}/roles")]
    public async Task<IActionResult> AssignRoles(long id, [FromBody] AssignRolesRequest request)
    {
        await _userService.AssignRolesAsync(id, request.Roles);
        return Ok(ApiResponse.Ok("角色分配成功"));
    }
}
