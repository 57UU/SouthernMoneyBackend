using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

/// <summary>
/// 商品分类数据访问层
/// </summary>
public class ProductCategoryRepository
{
    private readonly AppDbContext _context;
    
    public ProductCategoryRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 添加新商品分类
    /// </summary>
    public async Task<ProductCategory> AddCategoryAsync(ProductCategory category)
    {
        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }
    
    /// <summary>
    /// 根据ID获取商品分类
    /// </summary>
    public async Task<ProductCategory?> GetCategoryByIdAsync(Guid id)
    {
        return await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    /// <summary>
    /// 根据名称获取商品分类
    /// </summary>
    public async Task<ProductCategory?> GetCategoryByNameAsync(string name)
    {
        return await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Name == name);
    }
    
    /// <summary>
    /// 获取所有商品分类
    /// </summary>
    public async Task<List<ProductCategory>> GetAllCategoriesAsync()
    {
        return await _context.ProductCategories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
    
    /// <summary>
    /// 更新商品分类
    /// </summary>
    public async Task<ProductCategory> UpdateCategoryAsync(ProductCategory category)
    {
        _context.ProductCategories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }
    
    /// <summary>
    /// 删除商品分类
    /// </summary>
    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.ProductCategories.FindAsync(id);
        if (category == null)
        {
            return false;
        }
        
        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 检查分类名称是否已存在
    /// </summary>
    public async Task<bool> CategoryNameExistsAsync(string name)
    {
        return await _context.ProductCategories.AnyAsync(c => c.Name == name);
    }
    
    /// <summary>
    /// 检查分类是否存在
    /// </summary>
    public async Task<bool> CategoryExistsAsync(Guid id)
    {
        return await _context.ProductCategories.AnyAsync(c => c.Id == id);
    }
    
    /// <summary>
    /// 获取分类数量
    /// </summary>
    public async Task<int> GetCategoryCountAsync()
    {
        return await _context.ProductCategories.CountAsync();
    }
    
    /// <summary>
    /// 获取包含指定分类名称的所有分类
    /// </summary>
    public async Task<List<ProductCategory>> SearchCategoriesAsync(string keyword)
    {
        return await _context.ProductCategories
            .Where(c => c.Name.Contains(keyword))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}