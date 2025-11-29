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
}