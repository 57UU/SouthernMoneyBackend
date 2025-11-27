using Microsoft.EntityFrameworkCore;
using SouthernMoneyBackend;

namespace Database;

public class PostService
{
    private readonly AppDbContext dbContext;
    public PostService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    
    public async Task<Guid> CreatePostAsync(long userId, PostRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Title is required");
        }
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Content is required");
        }
        
        var post = new Post
        {
            Id = Guid.NewGuid(),
            UploaderUserId = userId,
            CreatedAt = DateTime.UtcNow,
            Title = request.Title,
            Content = request.Content,
            IsBlocked = false
        };
        
        // 处理标签
        if (request.Tags != null)
        {
            foreach (var tag in request.Tags.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                post.PostTags.Add(new PostTags
                {
                    PostId = post.Id,
                    Tag = tag.Trim()
                });
            }
        }
        
        // 处理图片
        if (request.Images != null)
        {
            foreach (var imageStr in request.Images)
            {
                if (!Guid.TryParse(imageStr, out var imageId))
                {
                    throw new ArgumentException($"Invalid image id: {imageStr}");
                }
                
                var imageExists = await dbContext.Images.AnyAsync(i => i.Id == imageId);
                if (!imageExists)
                {
                    throw new ArgumentException($"Image not found: {imageId}");
                }
                
                post.PostImages.Add(new PostImage
                {
                    PostId = post.Id,
                    ImageId = imageId
                });
            }
        }
        
        dbContext.Posts.Add(post);
        await dbContext.SaveChangesAsync();
        return post.Id;
    }
    
    public async Task<Post?> GetPostAsync(Guid postId)
    {
        return await dbContext.Posts
            .Include(p => p.User)
            .Include(p => p.PostImages)
            .Include(p => p.PostTags)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }
    
    public async Task<PostDto> GetPostDetailAsync(Guid postId, long currentUserId)
    {
        var post = await dbContext.Posts
            .Include(p => p.User)
            .Include(p => p.PostImages)
            .Include(p => p.PostTags)
            .FirstOrDefaultAsync(p => p.Id == postId)
            ?? throw new KeyNotFoundException("Post not found");
        
        // 增加浏览量
        post.ViewCount += 1;
        await dbContext.SaveChangesAsync();
        
        bool isLiked = await dbContext.PostLikes.AnyAsync(pl => pl.PostId == postId && pl.UserId == currentUserId);
        return MapToDto(post, isLiked);
    }
    
    public async Task<PaginatedResponse<PostDto>> GetPostsPageAsync(int page, int pageSize, long currentUserId)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        var query = dbContext.Posts
            .Include(p => p.User)
            .Include(p => p.PostImages)
            .Include(p => p.PostTags)
            .AsNoTracking();
        
        var totalCount = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var postIds = posts.Select(p => p.Id).ToList();
        var likedIds = await dbContext.PostLikes
            .Where(pl => pl.UserId == currentUserId && postIds.Contains(pl.PostId))
            .Select(pl => pl.PostId)
            .ToListAsync();
        
        var dtos = posts.Select(p => MapToDto(p, likedIds.Contains(p.Id))).ToList();
        return PaginatedResponse<PostDto>.Create(dtos, page, pageSize, totalCount);
    }
    
    public async Task<PaginatedResponse<PostDto>> GetMyPostsAsync(long userId, int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        var query = dbContext.Posts
            .Include(p => p.User)
            .Include(p => p.PostImages)
            .Include(p => p.PostTags)
            .Where(p => p.UploaderUserId == userId)
            .AsNoTracking();
        
        var totalCount = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var postIds = posts.Select(p => p.Id).ToList();
        var likedIds = await dbContext.PostLikes
            .Where(pl => pl.UserId == userId && postIds.Contains(pl.PostId))
            .Select(pl => pl.PostId)
            .ToListAsync();
        
        var dtos = posts.Select(p => MapToDto(p, likedIds.Contains(p.Id))).ToList();
        return PaginatedResponse<PostDto>.Create(dtos, page, pageSize, totalCount);
    }
    
    public async Task<int> LikePostAsync(Guid postId, long userId)
    {
        var post = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == postId)
            ?? throw new KeyNotFoundException("Post not found");
        
        var alreadyLiked = await dbContext.PostLikes.AnyAsync(pl => pl.PostId == postId && pl.UserId == userId);
        if (alreadyLiked)
        {
            return post.LikeCount;
        }
        
        dbContext.PostLikes.Add(new PostLike
        {
            PostId = postId,
            UserId = userId
        });
        post.LikeCount += 1;
        await dbContext.SaveChangesAsync();
        return post.LikeCount;
    }
    
    public async Task<int> ReportPostAsync(Guid postId, string reason)
    {
        var post = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == postId)
            ?? throw new KeyNotFoundException("Post not found");
        
        post.ReportCount += 1;
        await dbContext.SaveChangesAsync();
        return post.ReportCount;
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
