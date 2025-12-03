using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

/// <summary>
/// 用户资产数据访问层
/// </summary>
public class UserAssetRepository
{
    private readonly AppDbContext _context;
    
    public UserAssetRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 添加用户资产记录
    /// </summary>
    public async Task<UserAsset> AddUserAssetAsync(UserAsset asset)
    {
        _context.UserAssets.Add(asset);
        await _context.SaveChangesAsync();
        return asset;
    }
    
    /// <summary>
    /// 根据用户ID获取用户资产
    /// </summary>
    public async Task<UserAsset?> GetUserAssetByUserIdAsync(long userId)
    {
        return await _context.UserAssets
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.UserId == userId);
    }
    
    /// <summary>
    /// 更新用户资产
    /// </summary>
    public async Task<UserAsset> UpdateUserAssetAsync(UserAsset asset)
    {
        _context.UserAssets.Update(asset);
        await _context.SaveChangesAsync();
        return asset;
    }
    
    /// <summary>
    /// 删除用户资产记录
    /// </summary>
    public async Task<bool> DeleteUserAssetAsync(long userId)
    {
        var asset = await _context.UserAssets.FindAsync(userId);
        if (asset == null)
        {
            return false;
        }
        
        _context.UserAssets.Remove(asset);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 检查用户资产记录是否存在
    /// </summary>
    public async Task<bool> UserAssetExistsAsync(long userId)
    {
        return await _context.UserAssets.AnyAsync(a => a.UserId == userId);
    }
    
    /// <summary>
    /// 获取所有用户资产
    /// </summary>
    public async Task<List<UserAsset>> GetAllUserAssetsAsync()
    {
        return await _context.UserAssets
            .Include(a => a.User)
            .OrderByDescending(a => a.Total)
            .ToListAsync();
    }
    
    /// <summary>
    /// 分页获取用户资产
    /// </summary>
    public async Task<(List<UserAsset> Assets, int TotalCount)> GetUserAssetsPagedAsync(int page, int pageSize)
    {
        var query = _context.UserAssets
            .Include(a => a.User)
            .OrderByDescending(a => a.Total);
            
        var totalCount = await query.CountAsync();
        var assets = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (assets, totalCount);
    }
    
    /// <summary>
    /// 更新用户余额
    /// </summary>
    public async Task<bool> UpdateUserBalanceAsync(long userId, decimal newBalance)
    {
        var asset = await _context.UserAssets.FindAsync(userId);
        if (asset == null)
        {
            return false;
        }
        
        asset.Balance = newBalance;
        asset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 增加用户余额
    /// </summary>
    public async Task<bool> AddToUserBalanceAsync(long userId, decimal amount)
    {
        var asset = await _context.UserAssets.FindAsync(userId);
        if (asset == null)
        {
            return false;
        }
        
        asset.Balance += amount;
        asset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 减少用户余额
    /// </summary>
    public async Task<bool> SubtractFromUserBalanceAsync(long userId, decimal amount)
    {
        var asset = await _context.UserAssets.FindAsync(userId);
        if (asset == null || asset.Balance < amount)
        {
            return false;
        }
        
        asset.Balance -= amount;
        asset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 更新用户收益
    /// </summary>
    public async Task<bool> UpdateUserEarningsAsync(long userId, decimal todayEarn, decimal accumulatedEarn)
    {
        var asset = await _context.UserAssets.FindAsync(userId);
        if (asset == null)
        {
            return false;
        }
        
        asset.TodayEarn = todayEarn;
        asset.AccumulatedEarn = accumulatedEarn;
        asset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 获取用户资产总数
    /// </summary>
    public async Task<int> GetUserAssetCountAsync()
    {
        return await _context.UserAssets.CountAsync();
    }
}