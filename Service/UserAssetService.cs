using Microsoft.EntityFrameworkCore;
using Database.Repositories;
using Database;

namespace Service;

/// <summary>
/// 用户资产服务层
/// </summary>
public class UserAssetService
{
    private readonly UserAssetRepository _userAssetRepository;
    private readonly UserRepository _userRepository;
    
    public UserAssetService(
        UserAssetRepository userAssetRepository,
        UserRepository userRepository)
    {
        _userAssetRepository = userAssetRepository;
        _userRepository = userRepository;
    }
    
    /// <summary>
    /// 创建用户资产记录
    /// </summary>
    public async Task<UserAsset> CreateUserAssetAsync(long userId, decimal initialBalance = 0)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        // 检查用户资产记录是否已存在
        var existingAsset = await _userAssetRepository.GetUserAssetByUserIdAsync(userId);
        if (existingAsset != null)
        {
            throw new Exception("User asset already exists");
        }
        
        var asset = new UserAsset
        {
            UserId = userId,
            Total = initialBalance,
            TodayEarn = 0,
            AccumulatedEarn = 0,
            EarnRate = 0,
            Balance = initialBalance,
            UpdatedAt = DateTime.UtcNow
        };
        
        return await _userAssetRepository.AddUserAssetAsync(asset);
    }
    
    /// <summary>
    /// 获取用户资产
    /// </summary>
    public async Task<UserAsset?> GetUserAssetByUserIdAsync(long userId)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        return await _userAssetRepository.GetUserAssetByUserIdAsync(userId);
    }
    
    /// <summary>
    /// 更新用户资产
    /// </summary>
    public async Task<UserAsset> UpdateUserAssetAsync(UserAsset asset)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(asset.UserId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        asset.UpdatedAt = DateTime.UtcNow;
        return await _userAssetRepository.UpdateUserAssetAsync(asset);
    }
    
    /// <summary>
    /// 删除用户资产记录
    /// </summary>
    public async Task<bool> DeleteUserAssetAsync(long userId)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        return await _userAssetRepository.DeleteUserAssetAsync(userId);
    }
    
    /// <summary>
    /// 检查用户资产记录是否存在
    /// </summary>
    public async Task<bool> UserAssetExistsAsync(long userId)
    {
        return await _userAssetRepository.UserAssetExistsAsync(userId);
    }
    
    /// <summary>
    /// 获取所有用户资产
    /// </summary>
    public async Task<List<UserAsset>> GetAllUserAssetsAsync()
    {
        return await _userAssetRepository.GetAllUserAssetsAsync();
    }
    
    /// <summary>
    /// 分页获取用户资产
    /// </summary>
    public async Task<(List<UserAsset> Assets, int TotalCount)> GetUserAssetsPagedAsync(int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        return await _userAssetRepository.GetUserAssetsPagedAsync(page, pageSize);
    }
    
    /// <summary>
    /// 更新用户余额
    /// </summary>
    public async Task<bool> UpdateUserBalanceAsync(long userId, decimal newBalance)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        return await _userAssetRepository.UpdateUserBalanceAsync(userId, newBalance);
    }
    
    /// <summary>
    /// 增加用户余额
    /// </summary>
    public async Task<bool> AddToUserBalanceAsync(long userId, decimal amount)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        if (amount <= 0)
        {
            throw new Exception("Amount must be positive");
        }
        
        return await _userAssetRepository.AddToUserBalanceAsync(userId, amount);
    }
    
    /// <summary>
    /// 减少用户余额
    /// </summary>
    public async Task<bool> SubtractFromUserBalanceAsync(long userId, decimal amount)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        if (amount <= 0)
        {
            throw new Exception("Amount must be positive");
        }
        
        return await _userAssetRepository.SubtractFromUserBalanceAsync(userId, amount);
    }
    
    /// <summary>
    /// 更新用户收益
    /// </summary>
    public async Task<bool> UpdateUserEarningsAsync(long userId, decimal todayEarn, decimal accumulatedEarn)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        if (todayEarn < 0 || accumulatedEarn < 0)
        {
            throw new Exception("Earnings cannot be negative");
        }
        
        return await _userAssetRepository.UpdateUserEarningsAsync(userId, todayEarn, accumulatedEarn);
    }
    
    /// <summary>
    /// 获取用户资产总数
    /// </summary>
    public async Task<int> GetUserAssetCountAsync()
    {
        return await _userAssetRepository.GetUserAssetCountAsync();
    }
    
    /// <summary>
    /// 确保用户资产记录存在，如果不存在则创建
    /// </summary>
    public async Task<UserAsset> EnsureUserAssetExistsAsync(long userId, decimal initialBalance = 0)
    {
        var asset = await _userAssetRepository.GetUserAssetByUserIdAsync(userId);
        if (asset == null)
        {
            asset = await CreateUserAssetAsync(userId, initialBalance);
        }
        return asset;
    }
}