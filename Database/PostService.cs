using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Database;

public class PostService
{
    private readonly AppDbContext dbContext;
    public PostService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    
    public async Task<Guid> CreatePostAsync(long userId, string title, string content, ICollection<string>? images, ICollection<string>? tags)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required");
        }
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content is required");
        }
        
        var post = new Post
        {
            Id = Guid.NewGuid(),
            UploaderUserId = userId,
            CreatedAt = DateTime.UtcNow,
            Title = title,
            Content = content,
            IsBlocked = false
        };
        
        // 处理标签
        if (tags != null)
        {
            foreach (var tag in tags.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                post.PostTags.Add(new PostTags
                {
                    PostId = post.Id,
                    Tag = tag.Trim()
                });
            }
        }
        
        // 处理图片
        if (images != null)
        {
            foreach (var imageStr in images)
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
    
    public async Task<PostDetailResult> GetPostDetailAsync(Guid postId, long currentUserId)
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
        return new PostDetailResult
        {
            Post = post,
            IsLiked = isLiked
        };
    }
    
    public async Task<PagedPostsResult> GetPostsPageAsync(int page, int pageSize, long currentUserId)
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
        
        return new PagedPostsResult
        {
            Posts = posts,
            LikedPostIds = likedIds.ToHashSet(),
            TotalCount = totalCount
        };
    }
    
    public async Task<PagedPostsResult> GetMyPostsAsync(long userId, int page, int pageSize)
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
        
        return new PagedPostsResult
        {
            Posts = posts,
            LikedPostIds = likedIds.ToHashSet(),
            TotalCount = totalCount
        };
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
}

public class PostDetailResult
{
    public Post Post { get; set; } = null!;
    public bool IsLiked { get; set; }
}

public class PagedPostsResult
{
    public List<Post> Posts { get; set; } = new();
    public HashSet<Guid> LikedPostIds { get; set; } = new();
    public int TotalCount { get; set; }
}
