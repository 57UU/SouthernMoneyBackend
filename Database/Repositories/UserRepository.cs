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
    /// 细粒度更新：只更新用户名
    /// </summary>
    public async Task<bool> UpdateUserNameAsync(long userId, string newName)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return false;
            
        user.Name = newName;
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 细粒度更新：只更新用户邮箱
    /// </summary>
    public async Task<bool> UpdateUserEmailAsync(long userId, string? newEmail)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return false;
            
        user.Email = newEmail;
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 细粒度更新：只更新用户头像
    /// </summary>
    public async Task<bool> UpdateUserAvatarAsync(long userId, Guid newAvatarId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return false;
            
        user.Avatar = newAvatarId;
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 细粒度更新：使用Entity Framework的Property方法更新指定属性
    /// </summary>
    public async Task<bool> UpdateUserPropertyAsync<T>(long userId, string propertyName, T value)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;
            
        _context.Entry(user).Property(propertyName).CurrentValue = value;
        _context.Entry(user).Property(propertyName).IsModified = true;
        
        // 只标记指定属性为已修改，其他属性保持不变
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 细粒度更新：批量更新多个属性
    /// </summary>
    public async Task<bool> UpdateUserPropertiesAsync(long userId, Dictionary<string, object> properties)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;
            
        foreach (var property in properties)
        {
            _context.Entry(user).Property(property.Key).CurrentValue = property.Value;
            _context.Entry(user).Property(property.Key).IsModified = true;
        }
        
        await _context.SaveChangesAsync();
        return true;
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
    public async Task<(List<User> Users, int TotalCount)> GetUsersPagedAsync(int page, int pageSize, bool? isBlocked = null, bool? isAdmin = null, string? search = null)
    {
        var query = _context.Users.AsQueryable();
        
        // 应用筛选条件
        if (isBlocked.HasValue)
        {
            query = query.Where(u => u.IsBlocked == isBlocked.Value);
        }
        
        if (isAdmin.HasValue)
        {
            query = query.Where(u => u.IsAdmin == isAdmin.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => 
                u.Name.Contains(search) ||
                (u.Email != null && u.Email.Contains(search)));
        }
        
        var totalCount = await query.CountAsync();
        
        var users = await query
            .OrderByDescending(u => u.CreateTime)
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