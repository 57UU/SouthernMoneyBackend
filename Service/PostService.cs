using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Service;

public class PostService
{
    private readonly Database.Repositories.PostRepository postRepository;
    private readonly Database.Repositories.ImageRepository imageRepository;
    private readonly NotificationService notificationService;
    private readonly Database.Repositories.UserRepository userRepository;
    
    public PostService(Database.Repositories.PostRepository postRepository, Database.Repositories.ImageRepository imageRepository, NotificationService notificationService, Database.Repositories.UserRepository userRepository)
    {
        this.postRepository = postRepository;
        this.imageRepository = imageRepository;
        this.notificationService = notificationService;
        this.userRepository = userRepository;
    }
    
    public async Task<Guid> CreatePostAsync(long userId, string title, string content, ICollection<Guid>? images, ICollection<string>? tags)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required");
        }
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content is required");
        }
        
        var post = new Database.Post
        {
            Id = Guid.NewGuid(),
            UploaderUserId = userId,
            CreateTime = DateTime.UtcNow,
            Title = title,
            Content = content,
            IsBlocked = false
        };
        
        // 处理标签
        if (tags != null)
        {
            foreach (var tag in tags.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                post.PostTags.Add(new Database.PostTags
                {
                    PostId = post.Id,
                    Tag = tag.Trim()
                });
            }
        }
        
        // 处理图片
        if (images != null)
        {
            foreach (var imageId in images)
            {
                post.PostImages.Add(new Database.PostImage
                {
                    PostId = post.Id,
                    ImageId = imageId
                });
            }
        }
        
        await postRepository.AddPostAsync(post);
        return post.Id;
    }
    
    public async Task<Database.Post?> GetPostAsync(Guid postId)
    {
        return await postRepository.GetPostByIdAsync(postId);
    }
    
    public async Task<PostDetailResult> GetPostDetailAsync(Guid postId, long currentUserId)
    {
        // 增加浏览量
        await postRepository.IncrementPostViewCountAsync(postId);
        
        var post = await postRepository.GetPostByIdAsync(postId);
        if (post == null)
        {
            throw new KeyNotFoundException("Post not found");
        }
        
        bool isLiked = await postRepository.IsPostLikedByUserAsync(postId, currentUserId);
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
        
        var posts = await postRepository.GetAllPostsAsync(page, pageSize);
        var totalCount = await postRepository.GetPostCountAsync();
        
        await Task.WhenAll(posts.Select(post => postRepository.IncrementPostViewCountAsync(post.Id)));
        
        // 批量获取用户点赞的帖子ID，避免N+1查询问题
        var postIds = posts.Select(p => p.Id);
        var likedPostIds = await postRepository.GetUserLikedPostsAsync(postIds, currentUserId);
        
        return new PagedPostsResult
        {
            Posts = posts,
            LikedPostIds = likedPostIds,
            TotalCount = totalCount
        };
    }
    
    public async Task<PagedPostsResult> GetMyPostsAsync(long userId, int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        var posts = await postRepository.GetPostsByUserIdAsync(userId, page, pageSize);
        var totalCount = await postRepository.GetUserPostCountAsync(userId);
        
        // 批量获取用户点赞的帖子ID，避免N+1查询问题
        var postIds = posts.Select(p => p.Id);
        var likedPostIds = await postRepository.GetUserLikedPostsAsync(postIds, userId);
        
        return new PagedPostsResult
        {
            Posts = posts,
            LikedPostIds = likedPostIds,
            TotalCount = totalCount
        };
    }
    
    public async Task<int> LikePostAsync(Guid postId, long userId)
    {
        var alreadyLiked = await postRepository.IsPostLikedByUserAsync(postId, userId);
        if (alreadyLiked)
        {
            var post = await postRepository.GetPostByIdAsync(postId);
            return post?.LikeCount ?? 0;
        }
        
        // 获取帖子信息
        var postInfo = await postRepository.GetPostByIdAsync(postId);
        if (postInfo == null)
        {
            return 0;
        }
        
        // 如果不是自己点赞自己的帖子，发送通知
        if (postInfo.UploaderUserId != userId)
        {
            // 获取点赞用户信息
            var likeUser = await userRepository.GetUserByIdAsync(userId);
            string userName = likeUser?.Name ?? "某位用户";
            
            // 发送点赞通知
            await notificationService.CreateNotificationAsync(
                postInfo.UploaderUserId,
                $"{userName} 点赞了您的帖子《{postInfo.Title}》",
                "like",
                userId
            );
        }
        
        return await postRepository.AddPostLikeAsync(postId, userId);
    }
    
    public async Task<int> UnlikePostAsync(Guid postId, long userId)
    {
        var alreadyLiked = await postRepository.IsPostLikedByUserAsync(postId, userId);
        if (!alreadyLiked)
        {
            var post = await postRepository.GetPostByIdAsync(postId);
            return post?.LikeCount ?? 0;
        }
        
        // 获取帖子信息
        var postInfo = await postRepository.GetPostByIdAsync(postId);
        if (postInfo == null)
        {
            return 0;
        }
        
        // 如果不是自己取消点赞自己的帖子，发送通知
        if (postInfo.UploaderUserId != userId)
        {
            // 获取取消点赞用户信息
            var unlikeUser = await userRepository.GetUserByIdAsync(userId);
            string userName = unlikeUser?.Name ?? "某位用户";
            
            // 发送取消点赞通知
            await notificationService.CreateNotificationAsync(
                postInfo.UploaderUserId,
                $"{userName} 取消点赞了您的帖子《{postInfo.Title}》",
                "like",
                userId
            );
        }
        
        return await postRepository.RemovePostLikeAsync(postId, userId);
    }
    
    public async Task<int> ReportPostAsync(Guid postId, string reason)
    {
        // 获取帖子信息
        var postInfo = await postRepository.GetPostByIdAsync(postId);
        if (postInfo == null)
        {
            return 0;
        }
        
        // 举报帖子
        await postRepository.ReportPostAsync(postId);
        var post = await postRepository.GetPostByIdAsync(postId);
        
        // 发送举报通知给帖子作者
        await notificationService.CreateNotificationAsync(
            postInfo.UploaderUserId,
            $"您的帖子《{postInfo.Title}》已被举报。原因：{reason}",
            "system",
            null // 举报是系统行为，没有特定操作用户
        );
        
        return post?.ReportCount ?? 0;
    }
    
    public async Task<bool> DeletePostAsync(Guid postId, long userId)
    {
        var post = await postRepository.GetPostByIdAsync(postId);
        if (post == null)
        {
            return false;
        }
        
        // 只有帖子作者或管理员可以删除帖子
        if (post.UploaderUserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own posts");
        }
        
        return await postRepository.DeletePostAsync(postId);
    }
    
    public async Task<bool> UpdatePostAsync(Guid postId, long userId, string? title = null, string? content = null)
    {
        var post = await postRepository.GetPostByIdAsync(postId);
        if (post == null)
        {
            return false;
        }
        
        // 只有帖子作者可以编辑帖子
        if (post.UploaderUserId != userId)
        {
            throw new UnauthorizedAccessException("You can only edit your own posts");
        }
        
        if (!string.IsNullOrWhiteSpace(title))
        {
            post.Title = title;
        }
        
        if (!string.IsNullOrWhiteSpace(content))
        {
            post.Content = content;
        }
        
        await postRepository.UpdatePostAsync(post);
        return true;
    }
    
    public async Task<PagedPostsResult> SearchPostsAsync(string? query, string? tag = null, int page = 1, int pageSize = 10, long currentUserId = 0)
    {
        // 如果query为空，则不进行搜索过滤，只根据tag过滤或返回所有帖子
        if (string.IsNullOrWhiteSpace(query) && string.IsNullOrWhiteSpace(tag))
        {
            // 如果query和tag都为空，返回所有帖子
            return await GetPostsPageAsync(page, pageSize, currentUserId);
        }
        
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        var (posts, totalCount) = await postRepository.SearchPostsAsync(query, tag, page, pageSize);
        
        // 批量获取用户点赞的帖子ID，避免N+1查询问题
        var postIds = posts.Select(p => p.Id);
        var likedPostIds = await postRepository.GetUserLikedPostsAsync(postIds, currentUserId);
        
        return new PagedPostsResult
        {
            Posts = posts,
            LikedPostIds = likedPostIds,
            TotalCount = totalCount
        };
    }
}

public class PostDetailResult
{
    public Database.Post Post { get; set; } = null!;
    public bool IsLiked { get; set; }
}

public class PagedPostsResult
{
    public List<Database.Post> Posts { get; set; } = new();
    public HashSet<Guid> LikedPostIds { get; set; } = new();
    public int TotalCount { get; set; }
}