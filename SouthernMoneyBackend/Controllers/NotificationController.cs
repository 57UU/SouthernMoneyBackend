using Microsoft.AspNetCore.Mvc;
using Service;
using SouthernMoneyBackend.Middleware;
using SouthernMoneyBackend.Utils;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/notification")]
[AuthorizeUser]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;
    
    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    
    /// <summary>
    /// 获取用户的通知列表
    /// </summary>
    /// <param name="page">当前页码，默认1</param>
    /// <param name="pageSize">每页数量，默认10</param>
    /// <returns>通知列表</returns>
    [HttpGet("my")]
    public async Task<ApiResponse<PaginatedResponse<Database.Notification>>> GetMyNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var (total, notifications) = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
            
            return PaginatedResponse<Database.Notification>.CreateApiResponse(notifications, page, pageSize, total);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<Database.Notification>>.Fail(ex.Message);
        }
    }
    
    /// <summary>
    /// 获取用户未读通知数量
    /// </summary>
    /// <returns>未读通知数量</returns>
    [HttpGet("unread-count")]
    public async Task<ApiResponse<int>> GetUnreadCount()
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
            
            return ApiResponse<int>.Ok(count);
        }
        catch (Exception ex)
        {
            return ApiResponse<int>.Fail(ex.Message);
        }
    }
    
    /// <summary>
    /// 标记单个或多个通知为已读
    /// </summary>
    /// <param name="request">通知ID列表</param>
    /// <returns>操作结果</returns>
    [HttpPost("read")]
    public async Task<ApiResponse> MarkAsRead([FromBody] MarkReadRequest request)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            await _notificationService.MarkNotificationsAsReadAsync(userId, request.NotificationIds);
            
            return ApiResponse.Ok();
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail(ex.Message);
        }
    }
    
    /// <summary>
    /// 标记所有通知为已读
    /// </summary>
    /// <returns>操作结果</returns>
    [HttpPost("read-all")]
    public async Task<ApiResponse> MarkAllAsRead()
    {
        try
        {
            var userId = HttpContext.GetUserId();
            await _notificationService.MarkAllNotificationsAsReadAsync(userId);
            
            return ApiResponse.Ok();
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail(ex.Message);
        }
    }
    
    /// <summary>
    /// 标记已读请求参数
    /// </summary>
    public class MarkReadRequest
    {
        /// <summary>
        /// 通知ID列表
        /// </summary>
        public List<Guid> NotificationIds { get; set; } = new List<Guid>();
    }
}