using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

/// <summary>
/// 通知数据访问层
/// </summary>
public class NotificationRepository
{
    private readonly AppDbContext _context;
    
    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 获取用户的通知列表（分页）
    /// </summary>
    public async Task<(int total, List<Notification> notifications)> GetUserNotificationsAsync(long userId, int page, int pageSize)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreateTime);
        
        var total = await query.CountAsync();
        var notifications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (total, notifications);
    }
    
    /// <summary>
    /// 获取用户未读通知数量
    /// </summary>
    public async Task<int> GetUnreadNotificationCountAsync(long userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
    }
    
    /// <summary>
    /// 标记单个或多个通知为已读
    /// </summary>
    public async Task MarkNotificationsAsReadAsync(long userId, List<Guid> notificationIds)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && notificationIds.Contains(n.Id))
            .ToListAsync();
        
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }
        
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// 标记所有通知为已读
    /// </summary>
    public async Task MarkAllNotificationsAsReadAsync(long userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
        
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }
        
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// 添加新通知
    /// </summary>
    public async Task<Notification> AddNotificationAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }
}