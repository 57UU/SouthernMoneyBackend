using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using SouthernMoneyBackend.Utils;

namespace SouthernMoneyBackend.Middleware;

/// <summary>
/// 角色授权属性，用于验证用户是否具有指定角色
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    /// <summary>
    /// 构造函数，接受需要的角色
    /// </summary>
    /// <param name="roles">需要的角色列表</param>
    public AuthorizeRoleAttribute(params string[] roles)
    {
        _roles = roles;
    }

    /// <summary>
    /// 验证授权
    /// </summary>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // 检查用户是否已认证
        if (!context.HttpContext.Items.ContainsKey("User"))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var user = context.HttpContext.Items["User"] as ClaimsPrincipal;
        if (user == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // 检查用户是否具有指定角色
        bool hasRequiredRole = false;
        if (_roles.Length > 0)
        {
            hasRequiredRole = _roles.Any(role => user.IsInRole(role));
        }

        // 如果用户没有所需角色，返回403 Forbidden
        if (!hasRequiredRole)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}