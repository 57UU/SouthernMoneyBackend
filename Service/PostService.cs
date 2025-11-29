using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Service;

public class PostService
{
    private readonly Database.Repositories.PostRepository postRepository;
    private readonly Database.Repositories.ImageRepository imageRepository;
    
    public PostService(Database.Repositories.PostRepository postRepository, Database.Repositories.ImageRepository imageRepository)
    {
        this.postRepository = postRepository;
        this.imageRepository = imageRepository;
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
        
        var post = new Database.Post
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
            foreach (var imageStr in images)
            {
                if (!Guid.TryParse(imageStr, out var imageId))
                {
                    throw new ArgumentException($"Invalid image id: {imageStr}");
                }
                
                var imageExists = await imageRepository.GetImageByIdAsync(imageId);
                if (imageExists == null)
                {
                    throw new ArgumentException($"Image not found: {imageId}");
                }
                
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
        var post = await postRepository.GetPostByIdAsync(postId);
        if (post == null)
        {
            throw new KeyNotFoundException("Post not found");
        }
        
        // 增加浏览量
        await postRepository.IncrementPostViewCountAsync(postId);
        
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
        
        var postIds = posts.Select(p => p.Id).ToList();
        var likedPosts = new List<Database.PostLike>();
        
        foreach (var postId in postIds)
        {
            if (await postRepository.IsPostLikedByUserAsync(postId, currentUserId))
            {
                likedPosts.Add(new Database.PostLike { PostId = postId, UserId = currentUserId });
            }
        }
        
        var likedIds = likedPosts.Select(pl => pl.PostId).ToHashSet();
        
        return new PagedPostsResult
        {
            Posts = posts,
            LikedPostIds = likedIds,
            TotalCount = totalCount
        };
    }
    
    public async Task<PagedPostsResult> GetMyPostsAsync(long userId, int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        var posts = await postRepository.GetPostsByUserIdAsync(userId, page, pageSize);
        var totalCount = await postRepository.GetUserPostCountAsync(userId);
        
        var postIds = posts.Select(p => p.Id).ToList();
        var likedPosts = new List<Database.PostLike>();
        
        foreach (var postId in postIds)
        {
            if (await postRepository.IsPostLikedByUserAsync(postId, userId))
            {
                likedPosts.Add(new Database.PostLike { PostId = postId, UserId = userId });
            }
        }
        
        var likedIds = likedPosts.Select(pl => pl.PostId).ToHashSet();
        
        return new PagedPostsResult
        {
            Posts = posts,
            LikedPostIds = likedIds,
            TotalCount = totalCount
        };
    }
    
    public async Task<int> LikePostAsync(Guid postId, long userId)
    {
        var post = await postRepository.GetPostByIdAsync(postId);
        if (post == null)
        {
            throw new KeyNotFoundException("Post not found");
        }
        
        var alreadyLiked = await postRepository.IsPostLikedByUserAsync(postId, userId);
        if (alreadyLiked)
        {
            return post.LikeCount;
        }
        
        return await postRepository.AddPostLikeAsync(postId, userId);
    }
    
    public async Task<int> UnlikePostAsync(Guid postId, long userId)
    {
        var post = await postRepository.GetPostByIdAsync(postId);
        if (post == null)
        {
            throw new KeyNotFoundException("Post not found");
        }
        
        var alreadyLiked = await postRepository.IsPostLikedByUserAsync(postId, userId);
        if (!alreadyLiked)
        {
            return post.LikeCount;
        }
        
        return await postRepository.RemovePostLikeAsync(postId, userId);
    }
    
    public async Task<int> ReportPostAsync(Guid postId, string reason)
    {
        var post = await postRepository.GetPostByIdAsync(postId);
        if (post == null)
        {
            throw new KeyNotFoundException("Post not found");
        }
        
        await postRepository.ReportPostAsync(postId);
        return post.ReportCount;
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
        
        post.CreatedAt = DateTime.UtcNow;
        await postRepository.UpdatePostAsync(post);
        return true;
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