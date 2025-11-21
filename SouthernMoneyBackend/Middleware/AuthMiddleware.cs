using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Database;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SouthernMoneyBackend.Utils;

namespace SouthernMoneyBackend.Middleware;

/// <summary>
/// JWT认证中间件，验证Authorization请求头中的Bearer令牌
/// </summary>
public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    
    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context, UserService userService)
    {
        // 检查是否是登录相关的请求，如果是则跳过验证
        if (context.Request.Path.StartsWithSegments("/login"))
        {
            await _next(context);
            return;
        }
        
        // 获取Authorization请求头
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("Authorization header is required", "MISSING_AUTH_HEADER"));
            return;
        }
        
        string authHeaderValue = authHeader.ToString();
        // 验证Authorization头格式是否为Bearer token
        if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("Authorization header must be in format: Bearer {token}", "INVALID_AUTH_FORMAT"));
            return;
        }
        
        // 提取token
        string token = authHeaderValue.Substring(7).Trim();
        
        // 验证token并获取ClaimsPrincipal
        var principal = JwtUtils.ValidateToken(token);
        if (principal == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("Invalid or expired token", "INVALID_TOKEN"));
            return;
        }
        
        // 将用户信息存储在HttpContext中，以便后续控制器使用
        long? userId = JwtUtils.GetUserId(principal);
        bool isAdmin = JwtUtils.IsAdmin(principal);
        
        // 创建包含角色信息的新ClaimsIdentity
        var identity = new ClaimsIdentity(principal.Identity);
        
        // 添加基本用户角色
        identity.AddClaim(new Claim(ClaimTypes.Role, JwtUtils.USER_ROLE));
        
        // 如果是管理员，添加管理员角色
        if (isAdmin)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, JwtUtils.ADMIN_ROLE));
        }
        
        // 创建更新后的ClaimsPrincipal
        var updatedPrincipal = new ClaimsPrincipal(identity);
        
        context.Items["UserId"] = userId.Value;
        context.Items["IsAdmin"] = isAdmin;
        context.Items["User"] = updatedPrincipal; // 存储更新后的ClaimsPrincipal，包含角色信息
        
        await _next(context);
    }
}

/// <summary>
/// 中间件扩展方法，用于在Program.cs中注册中间件
/// </summary>
public static class AuthMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthMiddleware>();
    }
}