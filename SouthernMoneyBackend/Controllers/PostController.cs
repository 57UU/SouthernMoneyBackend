using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Database;
using SouthernMoneyBackend.Utils;
using SouthernMoneyBackend.Middleware;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/posts")]
[AuthorizeUser]
public class PostController : ControllerBase
{
    private readonly PostService postService;
    
    public PostController(PostService postService)
    {
        this.postService = postService;
    }
    
    // 公共接口示例 - 使用AllowAnonymous覆盖控制器级别的授权
    [HttpGet("public-list")]
    [AllowAnonymous]
    public IActionResult GetPublicPosts()
    {
        // 这里实现获取公开帖子的逻辑
        return Ok(ApiResponse.Ok(new { message = "这是一个不需要登录的公共接口" }));
    }
    
    // 用户级授权示例 - 普通用户可以访问
    [HttpGet("my-posts")]
    // 注意：这里不需要额外的授权特性，因为控制器级别已经应用了用户级授权
    public IActionResult GetMyPosts()
    {
        // 获取当前用户ID
        var userId = (long)HttpContext.Items["UserId"];
        // 这里实现获取当前用户帖子的逻辑
        return Ok(ApiResponse.Ok(new { userId = userId, message = "这是需要用户登录的接口" }));
    }
    
    // 管理员级授权示例 - 只有管理员可以访问
    [HttpGet("all-posts")]
    [AuthorizeRole("Admin")]
    public IActionResult GetAllPosts()
    {
        // 这里实现获取所有帖子的逻辑（管理员功能）
        return Ok(ApiResponse.Ok(new { message = "这是只有管理员可以访问的接口" }));
    }
    
    // 创建帖子 - 用户级授权
    [HttpPost]
    public IActionResult CreatePost([FromBody] dynamic postData)
    {
        var userId = (long)HttpContext.Items["UserId"];
        // 这里实现创建帖子的逻辑
        return Ok(ApiResponse.Ok(new { userId = userId, message = "帖子创建成功" }));
    }
    
    // 删除帖子 - 管理员级授权
    [HttpDelete("{postId}")]
    [AuthorizeRole("Admin")]
    public IActionResult DeletePost(Guid postId)
    {
        // 这里实现删除帖子的逻辑
        return Ok(ApiResponse.Ok(new { postId = postId, message = "帖子删除成功" }));
    }
}
