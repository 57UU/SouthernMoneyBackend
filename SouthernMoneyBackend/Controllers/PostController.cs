using Microsoft.AspNetCore.Mvc;
using Database;
using Service;
using SouthernMoneyBackend.Utils;
using SouthernMoneyBackend.Middleware;
using System.Linq;

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
    public async Task<ApiResponse<object>> CreatePost([FromBody] PostRequest request)
    {
        var userId = HttpContext.GetUserId();
        try
        {
            var postId = await postService.CreatePostAsync(userId, request.Title, request.Content, request.Images, request.Tags);
            return ApiResponse.Ok(new { PostId = postId });
        }
        catch (Exception e)
        {
            return ApiResponse.Fail(e.Message, "CREATE_POST_FAILED");
        }
    }
    
    [HttpGet("get")]
    public async Task<ApiResponse<PostDto>> GetPost([FromQuery(Name = "id")] Guid postId)
    {
        var userId = HttpContext.GetUserId();
        try
        {
            var detail = await postService.GetPostDetailAsync(postId, userId);
            var dto = MapToDto(detail.Post, detail.IsLiked);
            return ApiResponse<PostDto>.Ok(dto);
        }
        catch (KeyNotFoundException e)
        {
            return ApiResponse<PostDto>.Fail(e.Message, "POST_NOT_FOUND");
        }
        catch (Exception e)
        {
            return ApiResponse<PostDto>.Fail(e.Message, "GET_POST_FAILED");
        }
    }
    
    [HttpGet("page")]
    public async Task<ApiResponse<PaginatedResponse<PostDto>>> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = HttpContext.GetUserId();
        var result = await postService.GetPostsPageAsync(page, pageSize, userId);
        var dtos = result.Posts.Select(p => MapToDto(p, result.LikedPostIds.Contains(p.Id))).ToList();
        return PaginatedResponse<PostDto>.Create(dtos, page, pageSize, result.TotalCount);
    }
    
    [HttpGet("myPosts")]
    public async Task<ApiResponse<PaginatedResponse<PostDto>>> GetMyPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = HttpContext.GetUserId();
        var result = await postService.GetMyPostsAsync(userId, page, pageSize);
        var dtos = result.Posts.Select(p => MapToDto(p, result.LikedPostIds.Contains(p.Id))).ToList();
        return PaginatedResponse<PostDto>.Create(dtos, page, pageSize, result.TotalCount);
    }
    
    [HttpPost("like")]
    public async Task<ApiResponse<object>> LikePost([FromQuery(Name = "id")] Guid postId)
    {
        var userId = HttpContext.GetUserId();
        try
        {
            var likeCount = await postService.LikePostAsync(postId, userId);
            return ApiResponse.Ok(new { LikeCount = likeCount });
        }
        catch (KeyNotFoundException e)
        {
            return ApiResponse.Fail(e.Message, "POST_NOT_FOUND");
        }
        catch (Exception e)
        {
            return ApiResponse.Fail(e.Message, "LIKE_FAILED");
        }
    }
    
    [HttpPost("report")]
    public async Task<ApiResponse<object>> ReportPost([FromBody] PostReportRequest request)
    {
        try
        {
            var reportCount = await postService.ReportPostAsync(request.PostId, request.Reason);
            return ApiResponse.Ok(new { ReportCount = reportCount });
        }
        catch (KeyNotFoundException e)
        {
            return ApiResponse.Fail(e.Message, "POST_NOT_FOUND");
        }
        catch (Exception e)
        {
            return ApiResponse.Fail(e.Message, "REPORT_FAILED");
        }
    }
    
    private static PostDto MapToDto(Post post, bool isLiked)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            ReportCount = post.ReportCount,
            ViewCount = post.ViewCount,
            LikeCount = post.LikeCount,
            IsBlocked = post.IsBlocked,
            IsLiked = isLiked,
            Tags = post.PostTags?.Select(t => t.Tag).ToList() ?? new List<string>(),
            ImageIds = post.PostImages?.Select(pi => pi.ImageId).ToList() ?? new List<Guid>(),
            Uploader = post.User == null ? null : new PostUploaderDto
            {
                Id = post.User.Id,
                Name = post.User.Name
            }
        };
    }
}
