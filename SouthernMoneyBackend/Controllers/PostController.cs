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
    
    [HttpPost("create")]
    public async Task<IActionResult> CreatePost([FromBody] PostRequest request)
    {
        var userId = HttpContext.GetUserId();
        try
        {
            var postId = await postService.CreatePostAsync(userId, request);
            return Ok(ApiResponse.Ok(new { PostId = postId }));
        }
        catch (Exception e)
        {
            return BadRequest(ApiResponse.Fail(e.Message, "CREATE_POST_FAILED"));
        }
    }
    
    [HttpGet("get")]
    public async Task<IActionResult> GetPost([FromQuery(Name = "id")] Guid postId)
    {
        var userId = HttpContext.GetUserId();
        try
        {
            var post = await postService.GetPostDetailAsync(postId, userId);
            return Ok(ApiResponse.Ok(post));
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(ApiResponse.Fail(e.Message, "POST_NOT_FOUND"));
        }
        catch (Exception e)
        {
            return BadRequest(ApiResponse.Fail(e.Message, "GET_POST_FAILED"));
        }
    }
    
    [HttpGet("page")]
    public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = HttpContext.GetUserId();
        var result = await postService.GetPostsPageAsync(page, pageSize, userId);
        return Ok(result);
    }
    
    [HttpGet("myPosts")]
    public async Task<IActionResult> GetMyPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = HttpContext.GetUserId();
        var result = await postService.GetMyPostsAsync(userId, page, pageSize);
        return Ok(result);
    }
    
    [HttpPost("like")]
    public async Task<IActionResult> LikePost([FromQuery(Name = "id")] Guid postId)
    {
        var userId = HttpContext.GetUserId();
        try
        {
            var likeCount = await postService.LikePostAsync(postId, userId);
            return Ok(ApiResponse.Ok(new { LikeCount = likeCount }));
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(ApiResponse.Fail(e.Message, "POST_NOT_FOUND"));
        }
        catch (Exception e)
        {
            return BadRequest(ApiResponse.Fail(e.Message, "LIKE_FAILED"));
        }
    }
    
    [HttpPost("report")]
    public async Task<IActionResult> ReportPost([FromBody] PostReportRequest request)
    {
        try
        {
            var reportCount = await postService.ReportPostAsync(request.PostId, request.Reason);
            return Ok(ApiResponse.Ok(new { ReportCount = reportCount }));
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(ApiResponse.Fail(e.Message, "POST_NOT_FOUND"));
        }
        catch (Exception e)
        {
            return BadRequest(ApiResponse.Fail(e.Message, "REPORT_FAILED"));
        }
    }
}
