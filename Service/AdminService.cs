namespace Service;

public class AdminService
{
    private readonly Database.Repositories.UserRepository userRepository;
    private readonly Database.Repositories.ImageRepository imageRepository;
    private readonly Database.Repositories.PostRepository postRepository;

    public AdminService(Database.Repositories.UserRepository userRepository,
                        Database.Repositories.ImageRepository imageRepository,
                        Database.Repositories.PostRepository postRepository)
    {
        this.userRepository = userRepository;
        this.imageRepository = imageRepository;
        this.postRepository = postRepository;
    }
    
    /// <summary>
    /// 封禁用户
    /// </summary>
    public async Task BanUser(long userId, string reason)
    {
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return;
        }

        if (user.IsAdmin)
        {
            throw new Exception("Cannot ban administrator");
        }

        if (user.IsBlocked)
        {
            throw new Exception("User is already banned");
        }

        user.IsBlocked = true;
        user.BlockReason = reason;
        user.BlockedAt = DateTime.UtcNow;

        await userRepository.UpdateUserAsync(user);
    }

    /// <summary>
    /// 解封用户
    /// </summary>
    public async Task UnbanUser(long userId)
    {
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return;
        }

        if (!user.IsBlocked)
        {
            throw new Exception("User is not banned");
        }

        user.IsBlocked = false;
        user.BlockReason = null;
        user.BlockedAt = null;

        await userRepository.UpdateUserAsync(user);
    }
    public async Task SetAdmin(long userId,bool alreadyAdminOk = false){
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return;
        }
        if (user.IsAdmin && !alreadyAdminOk)
        {
            throw new Exception("User is already admin");
        }
        user.IsAdmin = true;
        await userRepository.UpdateUserAsync(user);
    }
    
    /// <summary>
    /// 检查用户是否被封禁（业务层可复用）
    /// </summary>
    public async Task EnsureUserNotBanned(long userId)
    {
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return;
        }

        if (user.IsBlocked)
        {
            throw new Exception($"User is banned: {user.BlockReason}");
        }
    }
    
    /// <summary>
    /// 获取被举报的帖子
    /// </summary>
    public async Task<(List<Database.Post> Posts, int TotalCount)> GetReportedPostsAsync(int page = 1, int pageSize = 10, bool isBlocked = false)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        
        return await postRepository.GetReportedPostsAsync(page, pageSize, isBlocked);
    }
    
    /// <summary>
    /// 处理举报帖子
    /// </summary>
    public async Task HandleReportAsync(Guid postId, bool isBlocked, string handleReason, long adminUserId)
    {
        if (isBlocked)
        {
            // 如果需要封禁帖子，记录封禁信息
            await postRepository.RecordPostBlockAsync(postId, adminUserId, handleReason);
        }
        else
        {
            // 如果不需要封禁，重置举报数并更新帖子状态
            await postRepository.ResetPostReportCountAsync(postId);
            await postRepository.TogglePostBlockStatusAsync(postId, false);
        }
    }
}