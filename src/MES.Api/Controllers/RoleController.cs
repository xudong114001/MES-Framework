using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;
using MES.Application.Interfaces;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "admin")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _service;
    public RoleController(IRoleService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        return Ok(ApiResponse.Ok(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var role = await _service.GetByIdAsync(id);
        if (role == null) return NotFound(ApiResponse.Fail("角色不存在"));
        return Ok(ApiResponse.Ok(role));
    }

    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetPermissions(long id)
    {
        var permissions = await _service.GetPermissionsAsync(id);
        return Ok(ApiResponse.Ok(permissions));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        try
        {
            await _service.CreateAsync(request.Name, request.Description);
            return Ok(ApiResponse.Ok("创建成功"));
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRoleRequest request)
    {
        await _service.UpdateAsync(id, request.Name, request.Description);
        return Ok(ApiResponse.Ok("更新成功"));
    }

    [HttpPut("{id}/permissions")]
    public async Task<IActionResult> AssignPermissions(long id, [FromBody] AssignPermissionsRequest request)
    {
        try
        {
            await _service.AssignPermissionsAsync(id, request.Permissions);
            return Ok(ApiResponse.Ok("权限分配成功"));
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse.Ok("删除成功"));
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
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
