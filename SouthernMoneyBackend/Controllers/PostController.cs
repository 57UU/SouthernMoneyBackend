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
            var postId = await postService.CreatePostAsync(userId, request.Title, request.Content, request.ImageIds, request.Tags);
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
            var dto = PostDto.FromPost(detail.Post, detail.IsLiked);
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
        var dtos = result.Posts.Select(p => PostDto.FromPost(p, result.LikedPostIds.Contains(p.Id))).ToList();
        return PaginatedResponse<PostDto>.CreateApiResponse(dtos, page, pageSize, result.TotalCount);
    }
    
    [HttpGet("myPosts")]
    public async Task<ApiResponse<PaginatedResponse<PostDto>>> GetMyPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = HttpContext.GetUserId();
        var result = await postService.GetMyPostsAsync(userId, page, pageSize);
        var dtos = result.Posts.Select(p => PostDto.FromPost(p, result.LikedPostIds.Contains(p.Id))).ToList();
        return PaginatedResponse<PostDto>.CreateApiResponse(dtos, page, pageSize, result.TotalCount);
    }
    
    [HttpPost("like")]
    public async Task<ApiResponse<object>> LikePost([FromQuery(Name = "id")] Guid postId)
    {
        var userId = HttpContext.GetUserId();
        try
        {
            var likeCount = await postService.LikePostAsync(postId, userId);
            return ApiResponse.Ok(new PostLikeResultDto { LikeCount = likeCount });
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
            return ApiResponse.Ok(new PostReportResultDto { ReportCount = reportCount });
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
    
    [HttpPost("delete")]
    public async Task<ApiResponse> DeletePost([FromBody] DeletePostRequest request)
    {
        var userId = HttpContext.GetUserId();
        try
        {
            await postService.DeletePostAsync(request.PostId, userId);
            return ApiResponse.Ok();
        }
        catch (KeyNotFoundException e)
        {
            return ApiResponse.Fail(e.Message, "POST_NOT_FOUND");
        }
        catch (UnauthorizedAccessException e)
        {
            return ApiResponse.Fail(e.Message, "UNAUTHORIZED");
        }
        catch (Exception e)
        {
            return ApiResponse.Fail(e.Message, "DELETE_POST_FAILED");
        }
    }
    
    [HttpPost("edit")]
    public async Task<ApiResponse> EditPost([FromBody] EditPostRequest request)
    {
        var userId = HttpContext.GetUserId();
        try
        {
            await postService.UpdatePostAsync(request.PostId, userId, request.Title, request.Content);
            return ApiResponse.Ok();
        }
        catch (KeyNotFoundException e)
        {
            return ApiResponse.Fail(e.Message, "POST_NOT_FOUND");
        }
        catch (UnauthorizedAccessException e)
        {
            return ApiResponse.Fail(e.Message, "UNAUTHORIZED");
        }
        catch (Exception e)
        {
            return ApiResponse.Fail(e.Message, "EDIT_POST_FAILED");
        }
    }
    
    // following is written by hr
    [HttpGet("search")]
    public async Task<ApiResponse<PaginatedResponse<PostDto>>> SearchPosts(
        [FromQuery(Name = "query")] string query,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "pageSize")] int pageSize = 10)
    {
        var userId = HttpContext.GetUserId();
        try
        {
            var result = await postService.SearchPostsAsync(query, page, pageSize, userId);
            var dtos = result.Posts.Select(p => PostDto.FromPost(p, result.LikedPostIds.Contains(p.Id))).ToList();
            return PaginatedResponse<PostDto>.CreateApiResponse(dtos, page, pageSize, result.TotalCount);
        }
        catch (ArgumentException e)
        {
            return ApiResponse<PaginatedResponse<PostDto>>.Fail(e.Message, "INVALID_SEARCH_QUERY");
        }
        catch (Exception e)
        {
            return ApiResponse<PaginatedResponse<PostDto>>.Fail(e.Message, "SEARCH_POSTS_FAILED");
        }
    }
}
