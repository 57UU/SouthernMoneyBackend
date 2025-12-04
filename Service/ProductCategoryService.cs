using Microsoft.EntityFrameworkCore;
using Database.Repositories;
using Database;

namespace Service;

/// <summary>
/// 商品分类服务层
/// </summary>
public class ProductCategoryService
{
    private readonly ProductCategoryRepository _categoryRepository;
    
    public ProductCategoryService(ProductCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    
    /// <summary>
    /// 创建新商品分类
    /// </summary>
    public async Task<ProductCategory> CreateCategoryAsync(string name, Guid coverImageId)
    {
        // 验证分类名称是否已存在
        var existingCategory = await _categoryRepository.GetCategoryByNameAsync(name);
        if (existingCategory != null)
        {
            throw new Exception("Category name already exists");
        }
        
        var category = new ProductCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            CoverImageId = coverImageId,
            CreateTime = DateTime.UtcNow
        };
        
        return await _categoryRepository.AddCategoryAsync(category);
    }
    
    /// <summary>
    /// 获取分类详情
    /// </summary>
    public async Task<ProductCategory?> GetCategoryByIdAsync(Guid id)
    {
        return await _categoryRepository.GetCategoryByIdAsync(id);
    }
    
    /// <summary>
    /// 根据名称获取分类
    /// </summary>
    public async Task<ProductCategory?> GetCategoryByNameAsync(string name)
    {
        return await _categoryRepository.GetCategoryByNameAsync(name);
    }
    
    /// <summary>
    /// 获取所有分类
    /// </summary>
    public async Task<List<ProductCategory>> GetAllCategoriesAsync()
    {
        return await _categoryRepository.GetAllCategoriesAsync();
    }
    
    /// <summary>
    /// 更新分类信息
    /// </summary>
    public async Task<ProductCategory> UpdateCategoryAsync(Guid id, string name, Guid coverImageId)
    {
        // 验证分类是否存在
        var category = await _categoryRepository.GetCategoryByIdAsync(id);
        if (category == null)
        {
            throw new Exception("Category not found");
        }
        
        // 如果名称有变化，检查新名称是否已存在
        if (category.Name != name)
        {
            var existingCategory = await _categoryRepository.GetCategoryByNameAsync(name);
            if (existingCategory != null && existingCategory.Id != id)
            {
                throw new Exception("Category name already exists");
            }
        }
        
        category.Name = name;
        category.CoverImageId = coverImageId;
        
        return await _categoryRepository.UpdateCategoryAsync(category);
    }
    
    /// <summary>
    /// 删除分类
    /// </summary>
    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        // 验证分类是否存在
        var category = await _categoryRepository.GetCategoryByIdAsync(id);
        if (category == null)
        {
            throw new Exception("Category not found");
        }
        
        return await _categoryRepository.DeleteCategoryAsync(id);
    }
    
    /// <summary>
    /// 检查分类名称是否已存在
    /// </summary>
    public async Task<bool> CategoryNameExistsAsync(string name)
    {
        return await _categoryRepository.CategoryNameExistsAsync(name);
    }
    
    /// <summary>
    /// 检查分类是否存在
    /// </summary>
    public async Task<bool> CategoryExistsAsync(Guid id)
    {
        return await _categoryRepository.CategoryExistsAsync(id);
    }
    
    /// <summary>
    /// 获取分类数量
    /// </summary>
    public async Task<int> GetCategoryCountAsync()
    {
        return await _categoryRepository.GetCategoryCountAsync();
    }
    
    /// <summary>
    /// 搜索分类
    /// </summary>
    public async Task<List<ProductCategory>> SearchCategoriesAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<ProductCategory>();
        }
        
        return await _categoryRepository.SearchCategoriesAsync(keyword);
    }
}