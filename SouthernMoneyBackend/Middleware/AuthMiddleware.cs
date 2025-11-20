using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Database;

namespace SouthernMoneyBackend.Middleware
{
    /// <summary>
    /// 鉴权中间件，验证请求头部的account和token字段
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
            
            // 获取请求头部的account和token字段
            if (!context.Request.Headers.TryGetValue("account", out var accountValues) ||
                !context.Request.Headers.TryGetValue("token", out var tokenValues))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Missing account or token in headers" });
                return;
            }
            
            string account = accountValues.ToString();
            string token = tokenValues.ToString();
            
            // 验证token并获取用户ID
            long? userId = await userService.GetUserIdByToken(token);
            if (userId == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid or expired token" });
                return;
            }
            
            // 将用户信息存储在HttpContext中，以便后续控制器使用
            context.Items["UserId"] = userId.Value;
            context.Items["Account"] = account;
            
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
}