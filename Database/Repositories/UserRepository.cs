using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

/// <summary>
/// 用户数据访问层
/// </summary>
public class UserRepository
{
    private readonly AppDbContext _context;
    
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 添加新用户
    /// </summary>
    public async Task<User> AddUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    public async Task<User?> GetUserByIdAsync(long id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }
    
    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    public async Task<User?> GetUserByNameAsync(string name)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
    }
    
    /// <summary>
    /// 更新用户信息
    /// </summary>
    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    /// <summary>
    /// 删除用户
    /// </summary>
    public async Task<bool> DeleteUserAsync(long id)
    {
        var user = await GetUserByIdAsync(id);
        if (user == null)
        {
            return false;
        }
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 检查用户名是否已存在
    /// </summary>
    public async Task<bool> UserNameExistsAsync(string name)
    {
        return await _context.Users.AnyAsync(u => u.Name == name);
    }
    
    /// <summary>
    /// 获取所有用户
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }
    
    /// <summary>
    /// 分页获取用户列表
    /// </summary>
    public async Task<(List<User> Users, int TotalCount)> GetUsersPagedAsync(int page, int pageSize, bool? isBlocked = null, bool? isAdmin = null)
    {
        var query = _context.Users.AsQueryable();
        
        if (isBlocked.HasValue)
        {
            query = query.Where(u => u.IsBlocked == isBlocked.Value);
        }
        
        if (isAdmin.HasValue)
        {
            query = query.Where(u => u.IsAdmin == isAdmin.Value);
        }
        
        var totalCount = await query.CountAsync();
        
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (users, totalCount);
    }
    
    /// <summary>
    /// 获取被封禁的用户数量
    /// </summary>
    public async Task<int> GetBannedUsersCountAsync()
    {
        return await _context.Users.CountAsync(u => u.IsBlocked);
    }

    /// <summary>
    /// 获取被封禁的用户数量（别名方法）
    /// </summary>
    public async Task<int> GetBannedUserCountAsync()
    {
        return await GetBannedUsersCountAsync();
    }

    /// <summary>
    /// 获取用户总数
    /// </summary>
    public async Task<int> GetUserCountAsync()
    {
        return await _context.Users.CountAsync();
    }
    
    /// <summary>
    /// 获取管理员数量
    /// </summary>
    public async Task<int> GetAdminUsersCountAsync()
    {
        return await _context.Users.CountAsync(u => u.IsAdmin);
    }
}