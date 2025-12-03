using Database.Repositories;
using Database;

namespace Service;

/// <summary>
/// 用户收藏分类服务层
/// </summary>
public class UserFavoriteCategoryService
{
    private readonly UserFavoriteCategoryRepository _favoriteCategoryRepository;
    private readonly UserRepository _userRepository;
    private readonly ProductCategoryRepository _categoryRepository;
    
    public UserFavoriteCategoryService(
        UserFavoriteCategoryRepository favoriteCategoryRepository,
        UserRepository userRepository,
        ProductCategoryRepository categoryRepository)
    {
        _favoriteCategoryRepository = favoriteCategoryRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
    }
    
    /// <summary>
    /// 添加收藏分类
    /// </summary>
    public async Task<UserFavoriteCategory> AddFavoriteCategoryAsync(long userId, Guid categoryId)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        // 验证分类是否存在
        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            throw new Exception("Category not found");
        }
        
        // 添加收藏
        return await _favoriteCategoryRepository.AddFavoriteCategoryAsync(userId, categoryId);
    }
    
    /// <summary>
    /// 取消收藏分类
    /// </summary>
    public async Task<bool> RemoveFavoriteCategoryAsync(long userId, Guid categoryId)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        // 验证分类是否存在
        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            throw new Exception("Category not found");
        }
        
        // 取消收藏
        return await _favoriteCategoryRepository.RemoveFavoriteCategoryAsync(userId, categoryId);
    }
    
    /// <summary>
    /// 获取用户的所有收藏分类
    /// </summary>
    public async Task<List<ProductCategory>> GetUserFavoriteCategoriesAsync(long userId)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        return await _favoriteCategoryRepository.GetUserFavoriteCategoriesAsync(userId);
    }
    
    /// <summary>
    /// 检查用户是否已收藏某个分类
    /// </summary>
    public async Task<bool> IsCategoryFavoritedAsync(long userId, Guid categoryId)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        // 验证分类是否存在
        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            throw new Exception("Category not found");
        }
        
        return await _favoriteCategoryRepository.IsCategoryFavoritedAsync(userId, categoryId);
    }
    
    /// <summary>
    /// 获取收藏某个分类的用户数量
    /// </summary>
    public async Task<int> GetFavoriteCountByCategoryAsync(Guid categoryId)
    {
        // 验证分类是否存在
        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            throw new Exception("Category not found");
        }
        
        return await _favoriteCategoryRepository.GetFavoriteCountByCategoryAsync(categoryId);
    }
}