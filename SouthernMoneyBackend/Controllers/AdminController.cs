using Microsoft.AspNetCore.Mvc;
using Service;
using SouthernMoneyBackend.Utils;
using Microsoft.AspNetCore.Authorization;
using SouthernMoneyBackend.Middleware;
using Database;
using Database.Repositories;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/admin")]
[AuthorizeAdmin]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;
    private readonly UserService _userService;
    private readonly UserRepository _userRepository;
    private readonly ProductRepository _productRepository;
    private readonly TransactionRepository _transactionRepository;

    public AdminController(
        AdminService adminService,
        UserService userService,
        UserRepository userRepository,
        ProductRepository productRepository,
        TransactionRepository transactionRepository)
    {
        _adminService = adminService;
        _userService = userService;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _transactionRepository = transactionRepository;
    }

    /// <summary>
    /// 封禁 / 解封 用户
    /// </summary>
    [HttpPost("handleUser")]
    public async Task<ApiResponse<object>> HandleUserAsync([FromBody] HandleUserRequest req)
    {
        if (req == null)
        {
            return ApiResponse<object>.Fail("Invalid request");
        }

        if (req.UserId <= 0)
        {
            return ApiResponse<object>.Fail("Invalid userId");
        }

        try
        {
            if (req.IsBlocked)
            {
                await _adminService.BanUser(req.UserId, req.HandleReason ?? "No reason");
            }
            else
            {
                await _adminService.UnbanUser(req.UserId);
            }

            return ApiResponse<object>.Ok(new { success = true, isBlocked = req.IsBlocked });
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 设置/取消用户为管理员
    /// </summary>
    [HttpPost("setAdmin")]
    public async Task<ApiResponse<object>> SetAdminAsync([FromBody] SetAdminRequest req)
    {
        if (req == null)
        {
            return ApiResponse<object>.Fail("Invalid request");
        }

        if (req.UserId <= 0)
        {
            return ApiResponse<object>.Fail("Invalid userId");
        }
        if(req.UserId==HttpContext.GetUserId())
        {
            return ApiResponse<object>.Fail("Cannot set self as admin");
        }

        try
        {
            await _adminService.SetAdmin(req.UserId);
            return ApiResponse.Ok();
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 获取用户列表
    /// </summary>
    [HttpGet("users")]
    public async Task<ApiResponse<PaginatedResponse<UserDto>>> GetUsersAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isBlocked = null,
        [FromQuery] bool? isAdmin = null,
        [FromQuery] string? search = null)
    {
        try
        {
            // 验证分页参数
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;

            // 直接从数据库获取筛选和分页后的用户列表
            var (users, totalCount) = await _userRepository.GetUsersPagedAsync(page, pageSize, isBlocked, isAdmin, search);

            // 转换为DTO
            var userDtos = UserDto.FromUserList(users);

            var paginatedResponse = PaginatedResponse<UserDto>.Create(
                userDtos, 
                totalCount, 
                page, 
                pageSize);

            return ApiResponse<PaginatedResponse<UserDto>>.Ok(paginatedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<UserDto>>.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 获取用户详情
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<ApiResponse<UserDetailDto>> GetUserAsync(long userId)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserDetailDto>.Fail("User not found");
            }

            var userDetailDto = UserDetailDto.FromUser(user);

            return ApiResponse<UserDetailDto>.Ok(userDetailDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDetailDto>.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 获取系统统计信息
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ApiResponse<SystemStatisticsDto>> GetStatisticsAsync()
    {
        try
        {
            var userCount = await _userRepository.GetUserCountAsync();
            var productCount = await _productRepository.GetProductCountAsync();
            var transactionCount = await _transactionRepository.GetTransactionCountAsync();

            // 获取最近30天的交易数量
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentTransactionCount = await _transactionRepository.GetTransactionCountSinceDateAsync(thirtyDaysAgo);

            // 获取被封禁用户数量
            var bannedUserCount = await _userRepository.GetBannedUserCountAsync();

            var statistics = new SystemStatisticsDto
            {
                TotalUsers = userCount,
                TotalProducts = productCount,
                TotalTransactions = transactionCount,
                RecentTransactions = recentTransactionCount,
                BannedUsers = bannedUserCount
            };

            return ApiResponse<SystemStatisticsDto>.Ok(statistics);
        }
        catch (Exception ex)
        {
            return ApiResponse<SystemStatisticsDto>.Fail(ex.Message);
        }
    }
    
    /// <summary>
    /// 查看被举报的帖子
    /// </summary>
    [HttpGet("reportedPosts")]
    public async Task<ApiResponse<PaginatedResponse<PostDto>>> GetReportedPostsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // 验证分页参数
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;

            // 获取被举报的帖子
            var (posts, totalCount) = await _adminService.GetReportedPostsAsync(page, pageSize);

            // 转换为DTO
            var postDtos = posts.Select(p => PostDto.FromPost(p, false)).ToList();

            var paginatedResponse = PaginatedResponse<PostDto>.Create(
                postDtos, 
                totalCount, 
                page, 
                pageSize);

            return ApiResponse<PaginatedResponse<PostDto>>.Ok(paginatedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<PostDto>>.Fail(ex.Message);
        }
    }
    
    /// <summary>
    /// 处理举报帖子
    /// </summary>
    [HttpPost("handleReport")]
    public async Task<ApiResponse<object>> HandleReportAsync([FromBody] HandleReportRequest req)
    {
        if (req == null)
        {
            return ApiResponse<object>.Fail("Invalid request");
        }

        if (req.PostId == Guid.Empty)
        {
            return ApiResponse<object>.Fail("Invalid postId");
        }

        try
        {
            // 获取当前管理员ID
            var adminUserId = HttpContext.GetUserId();
            await _adminService.HandleReportAsync(req.PostId, req.IsBlocked, req.HandleReason ?? "No reason", adminUserId);
            return ApiResponse.Ok();
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail(ex.Message);
        }
    }
}
