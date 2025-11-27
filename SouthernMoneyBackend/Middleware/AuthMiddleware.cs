using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Database;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SouthernMoneyBackend.Utils;

namespace SouthernMoneyBackend.Middleware;

/// <summary>
/// 认证中间件配置选项
/// </summary>
public class AuthMiddlewareOptions
{
    /// <summary>
    /// 是否启用认证中间件
    /// </summary>
    public bool Enable { get; set; } = true;
}

/// <summary>
/// JWT认证中间件，验证Authorization请求头中的Bearer令牌
/// </summary>
public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AuthMiddlewareOptions _options;
    
    public AuthMiddleware(RequestDelegate next, AuthMiddlewareOptions options)
    {
        _next = next;
        _options = options ?? new AuthMiddlewareOptions();
    }
    public async Task InvokeAsync(HttpContext context, UserService userService){
        if (_options.Enable)
        {
            await _verify(context, userService);
        }else{
            try{
                await _verify(context, userService);
            }catch(Exception){
                //ignore
            }
        }
        await _next(context);
    }
    public async Task _verify(HttpContext context, UserService userService)
    {
        // 登录与开放文档（swagger/openapi）不做认证
        if (context.Request.Path.StartsWithSegments("/login")
            || context.Request.Path.StartsWithSegments("/swagger")
            || context.Request.Path.StartsWithSegments("/openapi"))
        {
            return;
        }
        
        // 获取Authorization请求头
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("Authorization header is required"));
            return;
        }
        
        string authHeaderValue = authHeader.ToString();
        // 验证Authorization头格式是否为Bearer token
        if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("Authorization header must be in format: Bearer {token}"));
            return;
        }
        
        // 提取token
        string token = authHeaderValue.Substring(7).Trim();
        
        // 验证token并获取ClaimsPrincipal
        var principal = JwtUtils.ValidateToken(token);
        if (principal == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("Invalid or expired token"));
            return;
        }
        
        // 将用户信息存储在HttpContext中，以便后续控制器使用
        long? userId = JwtUtils.GetUserId(principal);
        bool isAdmin = JwtUtils.IsAdmin(principal);
        
        context.Items["UserId"] = userId;
        context.Items["IsAdmin"] = isAdmin;
        context.Items["User"] = principal;
    }
}

/// <summary>
/// 中间件扩展方法，用于在Program.cs中注册中间件
/// </summary>
public static class AuthMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthMiddleware>(new AuthMiddlewareOptions());
    }
    
    /// <summary>
    /// 注册认证中间件并配置选项
    /// </summary>
    /// <param name="builder">应用构建器</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>应用构建器</returns>
    public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder, Action<AuthMiddlewareOptions> configureOptions)
    {
        var options = new AuthMiddlewareOptions();
        configureOptions?.Invoke(options);
        return builder.UseMiddleware<AuthMiddleware>(options);
    }
}
