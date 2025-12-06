using Database.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service;

/// <summary>
/// 通知业务逻辑层
/// </summary>
public class NotificationService
{
    private readonly NotificationRepository _notificationRepository;
    
    public NotificationService(NotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }
    
    /// <summary>
    /// 获取用户的通知列表（分页）
    /// </summary>
    public async Task<(int total, List<Database.Notification> notifications)> GetUserNotificationsAsync(long userId, int page, int pageSize)
    {
        // 验证分页参数
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize; // 限制最大每页数量
        
        return await _notificationRepository.GetUserNotificationsAsync(userId, page, pageSize);
    }
    
    /// <summary>
    /// 获取用户未读通知数量
    /// </summary>
    public async Task<int> GetUnreadNotificationCountAsync(long userId)
    {
        return await _notificationRepository.GetUnreadNotificationCountAsync(userId);
    }
    
    /// <summary>
    /// 标记单个或多个通知为已读
    /// </summary>
    public async Task MarkNotificationsAsReadAsync(long userId, List<Guid> notificationIds)
    {
        if (notificationIds == null || notificationIds.Count == 0)
        {
            return;
        }
        
        await _notificationRepository.MarkNotificationsAsReadAsync(userId, notificationIds);
    }
    
    /// <summary>
    /// 标记所有通知为已读
    /// </summary>
    public async Task MarkAllNotificationsAsReadAsync(long userId)
    {
        await _notificationRepository.MarkAllNotificationsAsReadAsync(userId);
    }
    
    /// <summary>
    /// 创建通知
    /// </summary>
    public async Task<Database.Notification> CreateNotificationAsync(long userId, string content, string type = "system", long? subjectUserId = null)
    {
        var notification = new Database.Notification
        {
            UserId = userId,
            Content = content,
            Type = type,
            SubjectUserId = subjectUserId == userId ? null : subjectUserId
        };
        
        return await _notificationRepository.AddNotificationAsync(notification);
    }
}