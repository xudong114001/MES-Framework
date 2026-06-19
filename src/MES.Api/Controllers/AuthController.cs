using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Dtos;
using MES.Api.Middleware;
using MES.Api.Services;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(ApiResponse.Fail("用户名或密码错误"));
        return Ok(ApiResponse.Ok(result));
    }
}
