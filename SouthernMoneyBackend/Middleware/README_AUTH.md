# 授权系统使用说明

本文档详细介绍了SouthernMoneyBackend项目中的授权系统实现和使用方法。

## 授权特性概述

我们的系统提供了以下授权特性：

1. **AuthorizeRoleAttribute** - 基础授权特性，可指定需要的角色
2. **AuthorizeUserAttribute** - 用户级快捷授权特性（继承自AuthorizeRoleAttribute）
3. **AuthorizeAdminAttribute** - 管理员级快捷授权特性（继承自AuthorizeRoleAttribute）

## 授权特性使用方法

### 1. 控制器级别授权

您可以在控制器级别应用授权特性，这样该控制器下的所有操作方法都将受到授权保护：

```csharp
[ApiController]
[Route("/posts")]
[AuthorizeUser] // 应用用户级授权到整个控制器
public class PostController : ControllerBase
{
    // 所有操作方法都需要用户登录
}
```

### 2. 操作方法级别授权

您也可以在具体的操作方法上应用授权特性：

```csharp
[HttpGet("admin-data")]
[AuthorizeAdmin] // 只有管理员可以访问
public IActionResult GetAdminData()
{
    // 管理员操作逻辑
}
```

### 3. 覆盖控制器级别的授权

您可以使用`[AllowAnonymous]`特性覆盖控制器级别的授权要求，使特定操作方法不需要授权：

```csharp
[HttpGet("public-data")]
[AllowAnonymous] // 覆盖控制器级别的授权，允许匿名访问
public IActionResult GetPublicData()
{
    // 公共访问逻辑
}
```

### 4. 使用基础授权特性

如果需要更精细的角色控制，可以直接使用`AuthorizeRole`特性：

```csharp
[HttpGet("special-data")]
[AuthorizeRole("SpecialRole", "Admin")] // 需要SpecialRole或Admin角色
public IActionResult GetSpecialData()
{
    // 特殊角色访问逻辑
}
```

## 授权流程说明

1. **认证阶段**：
   - `AuthMiddleware`中间件验证请求头中的JWT令牌
   - 验证成功后，创建包含用户角色信息的`ClaimsPrincipal`
   - 将用户信息存储在`HttpContext.Items`中

2. **授权阶段**：
   - 授权特性（如`AuthorizeUser`）检查当前用户是否具有所需角色
   - 如果用户没有所需角色，返回403 Forbidden

## 获取当前用户信息

在控制器中，您可以通过`HttpContext.Items`获取当前登录用户的信息：

```csharp
[HttpGet("profile")]
[AuthorizeUser]
public IActionResult GetUserProfile()
{
    var userId = (long)HttpContext.Items["UserId"];
    var username = (string)HttpContext.Items["Username"];
    var isAdmin = (bool)HttpContext.Items["IsAdmin"];
    
    return Ok(new { userId, username, isAdmin });
}
```

## 授权错误处理

- 未提供有效的JWT令牌：返回401 Unauthorized
- 用户没有所需角色：返回403 Forbidden

## 最佳实践

1. 对敏感操作始终使用适当的授权保护
2. 使用控制器级别授权作为默认保护，只对需要的操作使用`[AllowAnonymous]`
3. 对管理员功能使用`[AuthorizeAdmin]`确保安全性
4. 避免在授权检查中使用硬编码的角色名称，使用我们提供的快捷特性

## 代码示例

请参考`PostController.cs`中的完整示例实现。