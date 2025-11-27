using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SouthernMoneyBackend.Utils;
using System.Net;
using System.Text.Json;

namespace SouthernMoneyBackend.Middleware;

public class ExceptionHandlerMiddlewareOptions
{
    public bool IncludeExceptionDetailsInProduction { get; set; } = false;
}

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly ExceptionHandlerMiddlewareOptions _options;
    
    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger, ExceptionHandlerMiddlewareOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("The response has already started, unable to handle exception: {Message}", exception.Message);
            return;
        }

        context.Response.ContentType = "application/json";
        
        // 根据不同类型的异常设置不同的状态码和错误信息
        (HttpStatusCode statusCode, string message, string errorCode) = exception switch
        {
            ArgumentNullException or ArgumentException => 
                (HttpStatusCode.BadRequest, exception.Message, "INVALID_PARAMETER"),
            KeyNotFoundException => 
                (HttpStatusCode.NotFound, exception.Message, "RESOURCE_NOT_FOUND"),
            UnauthorizedAccessException => 
                (HttpStatusCode.Unauthorized, "Unauthorized access", "UNAUTHORIZED"),
            AccessViolationException => 
                (HttpStatusCode.Forbidden, "Access denied", "FORBIDDEN"),
            _ => 
                (HttpStatusCode.InternalServerError, "An unexpected error occurred", "INTERNAL_ERROR")
        };
        
        context.Response.StatusCode = (int)statusCode;
        
        // 创建标准的错误响应
        var response = ApiResponse.Fail(message, errorCode);
        
        // 如果是开发环境，包含详细的异常信息
        if (_options.IncludeExceptionDetailsInProduction)
        {
            var devResponse = new
            {
                response.Success,
                response.Message,
                response.ErrorCode,
                statusCode,
                response.Timestamp,
                Details = exception.ToString(),
                StackTrace = exception.StackTrace
            };
            await context.Response.WriteAsJsonAsync(devResponse);
        }
        else
        {
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}

/// <summary>
/// 中间件扩展方法
/// </summary>
public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
