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
            .OrderByDescending(p => p.CreateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// 获取用户的帖子
    /// </summary>
    public async Task<List<Post>> GetPostsByUserIdAsync(long userId, int page = 1, int pageSize = 10, bool includeBlocked = false)
    {
        var query = _context.Posts
            .Include(p => p.User)
            .Include(p => p.PostImages)
            .ThenInclude(pi => pi.Image)
            .Include(p => p.PostTags)
            .Include(p => p.PostLikes)
            .Where(p => p.UploaderUserId == userId);
            
        if (!includeBlocked)
        {
            query = query.Where(p => !p.IsBlocked);
        }
        
        return await query
            .OrderByDescending(p => p.CreateTime)
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
    /// 批量检查用户对多个帖子的点赞状态
    /// </summary>
    public async Task<HashSet<Guid>> GetUserLikedPostsAsync(IEnumerable<Guid> postIds, long userId)
    {
        return await _context.PostLikes
            .Where(pl => postIds.Contains(pl.PostId) && pl.UserId == userId)
            .Select(pl => pl.PostId)
            .ToHashSetAsync();
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
    /// 记录帖子处理历史
    /// </summary>
    public async Task<bool> RecordPostHandleHistoryAsync(Guid postId, long adminUserId, string handleReason, bool isBlocked)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            return false;
        }
        
        // 创建新的处理记录
        var postBlock = new PostBlock
        {
            PostId = postId,
            AdminUserId = adminUserId,
            IsBlock = isBlocked,
            Reason = handleReason,
            ActionTime = DateTime.UtcNow
        };
        
        _context.PostBlocks.Add(postBlock);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 重置帖子举报数
    /// </summary>
    public async Task<bool> ResetPostReportCountAsync(Guid postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            return false;
        }
        
        post.ReportCount = 0;
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
    public async Task<int> GetUserPostCountAsync(long userId, bool includeBlocked = false)
    {
        var query = _context.Posts.Where(p => p.UploaderUserId == userId);
        
        if (!includeBlocked)
        {
            query = query.Where(p => !p.IsBlocked);
        }
        
        return await query.CountAsync();
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
    
    /// <summary>
    /// 搜索帖子
    /// </summary>
    public async Task<(List<Post> Posts, int TotalCount)> SearchPostsAsync(string? query, string? tag = null, int page = 1, int pageSize = 10)
    {
        var postsQuery = _context.Posts
            .Include(p => p.User)
            .Include(p => p.PostImages)
            .ThenInclude(pi => pi.Image)
            .Include(p => p.PostTags)
            .Include(p => p.PostLikes)
            .Where(p => !p.IsBlocked);
        
        // 如果提供了query参数，进行搜索过滤
        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.Trim().ToLower();
            postsQuery = postsQuery.Where(p => 
                p.Title.ToLower().Contains(normalizedQuery) || 
                p.Content.ToLower().Contains(normalizedQuery) ||
                p.PostTags.Any(pt => pt.Tag.ToLower().Contains(normalizedQuery)));
        }
        
        // 如果提供了tag参数，进一步过滤结果
        if (!string.IsNullOrWhiteSpace(tag))
        {
            var normalizedTag = tag.Trim().ToLower();
            postsQuery = postsQuery.Where(p => p.PostTags.Any(pt => pt.Tag.ToLower() == normalizedTag));
        }
        
        // 按时间倒序排列
        postsQuery = postsQuery.OrderByDescending(p => p.CreateTime);
        
        var totalCount = await postsQuery.CountAsync();
        
        var posts = await postsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (posts, totalCount);
    }
    
    /// <summary>
    /// 获取被举报的帖子（分页）
    /// </summary>
    public async Task<(List<Post> Posts, int TotalCount)> GetReportedPostsAsync(int page = 1, int pageSize = 10, bool isBlocked = false)
    {
        var postsQuery = _context.Posts
            .Include(p => p.User)
            .Include(p => p.PostImages)
            .ThenInclude(pi => pi.Image)
            .Include(p => p.PostTags)
            .Include(p => p.PostLikes)
            .Include(p => p.PostBlocks)
            .ThenInclude(pb => pb.AdminUser)
            .Where(p => p.ReportCount > 0);
        
        // 根据 isBlocked 参数过滤
        if (isBlocked)
        {
            postsQuery = postsQuery.Where(p => p.IsBlocked);
        }
        else
        {
            postsQuery = postsQuery.Where(p => !p.IsBlocked);
        }
        
        postsQuery = postsQuery
            .OrderByDescending(p => p.ReportCount)
            .ThenByDescending(p => p.CreateTime);
        
        var totalCount = await postsQuery.CountAsync();
        
        var posts = await postsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (posts, totalCount);
    }
}