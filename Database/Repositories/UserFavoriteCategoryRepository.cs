using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

/// <summary>
/// 用户收藏分类数据访问层
/// </summary>
public class UserFavoriteCategoryRepository
{
    private readonly AppDbContext _context;
    
    public UserFavoriteCategoryRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 添加收藏分类
    /// </summary>
    public async Task<UserFavoriteCategory> AddFavoriteCategoryAsync(long userId, Guid categoryId)
    {
        // 检查是否已经收藏
        var existingFavorite = await _context.UserFavoriteCategories
            .FirstOrDefaultAsync(f => f.UserId == userId && f.CategoryId == categoryId);
        
        if (existingFavorite != null)
        {
            throw new Exception("Category already favorited");
        }
        
        var favoriteCategory = new UserFavoriteCategory
        {
            UserId = userId,
            CategoryId = categoryId,
            CreateTime = DateTime.UtcNow
        };
        
        _context.UserFavoriteCategories.Add(favoriteCategory);
        await _context.SaveChangesAsync();
        return favoriteCategory;
    }
    
    /// <summary>
    /// 取消收藏分类
    /// </summary>
    public async Task<bool> RemoveFavoriteCategoryAsync(long userId, Guid categoryId)
    {
        var favoriteCategory = await _context.UserFavoriteCategories
            .FirstOrDefaultAsync(f => f.UserId == userId && f.CategoryId == categoryId);
        
        if (favoriteCategory == null)
        {
            return false;
        }
        
        _context.UserFavoriteCategories.Remove(favoriteCategory);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 获取用户的所有收藏分类
    /// </summary>
    public async Task<List<ProductCategory>> GetUserFavoriteCategoriesAsync(long userId)
    {
        return await _context.UserFavoriteCategories
            .Where(f => f.UserId == userId)
            .Include(f => f.Category)
                .ThenInclude(c => c.FavoriteUsers)
            .OrderByDescending(f => f.CreateTime)
            .Select(f => f.Category)
            .ToListAsync();
    }
    
    /// <summary>
    /// 检查用户是否已收藏某个分类
    /// </summary>
    public async Task<bool> IsCategoryFavoritedAsync(long userId, Guid categoryId)
    {
        return await _context.UserFavoriteCategories
            .AnyAsync(f => f.UserId == userId && f.CategoryId == categoryId);
    }
    
    /// <summary>
    /// 获取收藏某个分类的用户数量
    /// </summary>
    public async Task<int> GetFavoriteCountByCategoryAsync(Guid categoryId)
    {
        return await _context.UserFavoriteCategories
            .CountAsync(f => f.CategoryId == categoryId);
    }
    
    /// <summary>
    /// 获取用户收藏分类的数量
    /// </summary>
    public async Task<int> GetFavoriteCountByUserAsync(long userId)
    {
        return await _context.UserFavoriteCategories
            .CountAsync(f => f.UserId == userId);
    }
}