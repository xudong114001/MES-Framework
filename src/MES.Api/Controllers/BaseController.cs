using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MES.Api.Middleware;

namespace MES.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BaseController : ControllerBase
{
    /// <summary>
    /// 获取当前登录用户的 ID
    /// </summary>
    protected long CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? long.Parse(claim.Value) : 0;
        }
    }

    /// <summary>
    /// 获取当前登录用户的用户名
    /// </summary>
    protected string CurrentUserName => User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

    /// <summary>
    /// 成功响应
    /// </summary>
    protected IActionResult Success(object? data = null) => Ok(ApiResponse.Ok(data));

    /// <summary>
    /// 失败响应
    /// </summary>
    protected IActionResult Fail(string message, int statusCode = 400) =>
        statusCode switch
        {
            400 => BadRequest(ApiResponse.Fail(message)),
            404 => NotFound(ApiResponse.Fail(message)),
            403 => Forbid(),
            _ => StatusCode(statusCode, ApiResponse.Fail(message))
        };
}
