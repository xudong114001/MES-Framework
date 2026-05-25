using System.Net;
using System.Text.Json;
using Serilog;

namespace MES.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception: {Message}", ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new ApiResponse(500, "服务器内部错误");
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

public class ApiResponse
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    public ApiResponse() { }

    public ApiResponse(int code, string message, object? data = null)
    {
        Code = code;
        Message = message;
        Data = data;
    }

    public static ApiResponse Ok(object? data = null) => new(0, "success", data);
    public static ApiResponse Fail(string message) => new(1, message);
}
