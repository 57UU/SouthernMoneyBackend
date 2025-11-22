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
    [HttpGet("get")]
    [AllowAnonymous]
    public IActionResult GetPost([FromQuery(Name = "id")] Guid postId)
    {
        throw new NotImplementedException();
    }
    
    // 用户级授权示例 - 普通用户可以访问
    [HttpGet("create")]
    public IActionResult GetMyPosts()
    {
        var userId = HttpContext.GetUserId();
        throw new NotImplementedException();
    }
}
