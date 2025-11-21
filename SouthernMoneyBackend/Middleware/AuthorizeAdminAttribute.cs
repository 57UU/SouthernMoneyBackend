using Microsoft.AspNetCore.Mvc.Filters;

namespace SouthernMoneyBackend.Middleware;

/// <summary>
/// 管理员级验证特性快捷方式
/// 用于验证用户是否具有管理员权限
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class AuthorizeAdminAttribute : AuthorizeRoleAttribute
{
    /// <summary>
    /// 构造函数，默认使用"Admin"角色
    /// </summary>
    public AuthorizeAdminAttribute() : base("Admin")
    {}
}