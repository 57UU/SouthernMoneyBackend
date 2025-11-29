using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

/// <summary>
/// 帖子数据访问层
/// </summary>
public class PostRepository
{
    private readonly AppDbContext _context;
    
    public PostRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 添加新帖子
    /// </summary>
    public async Task<Post> AddPostAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }
    
    /// <summary>
    /// 根据ID获取帖子
    /// </summary>
    public async Task<Post?> GetPostByIdAsync(Guid id)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.PostImages)
            .ThenInclude(pi => pi.Image)
            .Include(p => p.PostTags)
            .Include(p => p.PostLikes)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    /// <summary>
    /// 更新帖子
    /// </summary>
    public async Task<Post> UpdatePostAsync(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
        return post;
    }
    
    /// <summary>
    /// 删除帖子
    /// </summary>
    public async Task<bool> DeletePostAsync(Guid id)
    {
        var post = await GetPostByIdAsync(id);
        if (post == null)
        {
            return false;
        }
        
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 获取所有帖子（分页）
    /// </summary>
    public async Task<List<Post>> GetAllPostsAsync(int page = 1, int pageSize = 10)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.PostImages)
            .ThenInclude(pi => pi.Image)
            .Include(p => p.PostTags)
            .Include(p => p.PostLikes)
            .Where(p => !p.IsBlocked)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// 获取用户的帖子
    /// </summary>
    public async Task<List<Post>> GetPostsByUserIdAsync(long userId, int page = 1, int pageSize = 10)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.PostImages)
            .ThenInclude(pi => pi.Image)
            .Include(p => p.PostTags)
            .Include(p => p.PostLikes)
            .Where(p => p.UploaderUserId == userId && !p.IsBlocked)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// 添加帖子点赞
    /// </summary>
    public async Task<int> AddPostLikeAsync(Guid postId, long userId)
    {
        // 检查是否已经点赞
        var alreadyLiked = await _context.PostLikes
            .AnyAsync(pl => pl.PostId == postId && pl.UserId == userId);
        
        if (alreadyLiked)
        {
            var post = await _context.Posts.FindAsync(postId);
            return post?.LikeCount ?? 0;
        }
        
        _context.PostLikes.Add(new PostLike
        {
            PostId = postId,
            UserId = userId
        });
        
        // 更新帖子的点赞数
        var postToUpdate = await _context.Posts.FindAsync(postId);
        if (postToUpdate != null)
        {
            postToUpdate.LikeCount++;
            _context.Posts.Update(postToUpdate);
        }
        
        await _context.SaveChangesAsync();
        return postToUpdate?.LikeCount ?? 0;
    }
    
    /// <summary>
    /// 移除帖子点赞
    /// </summary>
    public async Task<int> RemovePostLikeAsync(Guid postId, long userId)
    {
        var postLike = await _context.PostLikes
            .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);
        
        if (postLike == null)
        {
            var post = await _context.Posts.FindAsync(postId);
            return post?.LikeCount ?? 0;
        }
        
        _context.PostLikes.Remove(postLike);
        
        // 更新帖子的点赞数
        var postToUpdate = await _context.Posts.FindAsync(postId);
        if (postToUpdate != null && postToUpdate.LikeCount > 0)
        {
            postToUpdate.LikeCount--;
            _context.Posts.Update(postToUpdate);
        }
        
        await _context.SaveChangesAsync();
        return postToUpdate?.LikeCount ?? 0;
    }
    
    /// <summary>
    /// 检查用户是否已点赞帖子
    /// </summary>
    public async Task<bool> IsPostLikedByUserAsync(Guid postId, long userId)
    {
        return await _context.PostLikes
            .AnyAsync(pl => pl.PostId == postId && pl.UserId == userId);
    }
    
    /// <summary>
    /// 增加帖子浏览量
    /// </summary>
    public async Task IncrementPostViewCountAsync(Guid postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post != null)
        {
            post.ViewCount++;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }
    }
    
    /// <summary>
    /// 举报帖子
    /// </summary>
    public async Task<bool> ReportPostAsync(Guid postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            return false;
        }
        
        post.ReportCount++;
        
        // 如果举报数超过阈值，自动屏蔽帖子
        if (post.ReportCount >= 5)
        {
            post.IsBlocked = true;
        }
        
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 屏蔽/取消屏蔽帖子
    /// </summary>
    public async Task<bool> TogglePostBlockStatusAsync(Guid postId, bool isBlocked)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            return false;
        }
        
        post.IsBlocked = isBlocked;
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 添加帖子标签
    /// </summary>
    public async Task<PostTags> AddPostTagAsync(PostTags postTag)
    {
        _context.PostTags.Add(postTag);
        await _context.SaveChangesAsync();
        return postTag;
    }
    
    /// <summary>
    /// 移除帖子标签
    /// </summary>
    public async Task<bool> RemovePostTagAsync(Guid postId, string tag)
    {
        var postTag = await _context.PostTags
            .FirstOrDefaultAsync(pt => pt.PostId == postId && pt.Tag == tag);
        
        if (postTag == null)
        {
            return false;
        }
        
        _context.PostTags.Remove(postTag);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 获取所有帖子数量
    /// </summary>
    public async Task<int> GetPostCountAsync()
    {
        return await _context.Posts
            .Where(p => !p.IsBlocked)
            .CountAsync();
    }
    
    /// <summary>
    /// 获取用户帖子数量
    /// </summary>
    public async Task<int> GetUserPostCountAsync(long userId)
    {
        return await _context.Posts
            .Where(p => p.UploaderUserId == userId && !p.IsBlocked)
            .CountAsync();
    }
    
    /// <summary>
    /// 添加帖子图片关联
    /// </summary>
    public async Task<PostImage> AddPostImageAsync(PostImage postImage)
    {
        _context.PostImages.Add(postImage);
        await _context.SaveChangesAsync();
        return postImage;
    }
    
    /// <summary>
    /// 移除帖子图片关联
    /// </summary>
    public async Task<bool> RemovePostImageAsync(Guid postId, Guid imageId)
    {
        var postImage = await _context.PostImages
            .FirstOrDefaultAsync(pi => pi.PostId == postId && pi.ImageId == imageId);
        
        if (postImage == null)
        {
            return false;
        }
        
        _context.PostImages.Remove(postImage);
        await _context.SaveChangesAsync();
        return true;
    }
}