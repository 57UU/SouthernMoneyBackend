using Microsoft.AspNetCore.Mvc.Filters;

namespace SouthernMoneyBackend.Middleware;

/// <summary>
/// 用户级验证特性快捷方式
/// 用于验证用户是否已登录（普通用户权限）
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class AuthorizeUserAttribute : AuthorizeRoleAttribute
{
    /// <summary>
    /// 构造函数，默认使用"User"角色
    /// </summary>
    public AuthorizeUserAttribute() : base("User")
    {}
}