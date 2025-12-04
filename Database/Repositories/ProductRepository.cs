using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

/// <summary>
/// 商品数据访问层
/// </summary>
public class ProductRepository
{
    private readonly AppDbContext _context;
    
    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 添加新商品
    /// </summary>
    public async Task<Product> AddProductAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }
    
    /// <summary>
    /// 根据ID获取商品
    /// </summary>
    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.User)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }
    
    /// <summary>
    /// 获取用户的所有商品
    /// </summary>
    public async Task<List<Product>> GetProductsByUserIdAsync(long userId)
    {
        return await _context.Products
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(p => p.UploaderUserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreateTime)
            .ToListAsync();
    }
    
    /// <summary>
    /// 分页获取用户的商品
    /// </summary>
    public async Task<(List<Product> Products, int TotalCount)> GetProductsByUserIdPagedAsync(long userId, int page, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(p => p.UploaderUserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreateTime);
            
        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (products, totalCount);
    }
    
    /// <summary>
    /// 获取所有商品
    /// </summary>
    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreateTime)
            .ToListAsync();
    }
    
    /// <summary>
    /// 分页获取商品
    /// </summary>
    public async Task<(List<Product> Products, int TotalCount)> GetProductsPagedAsync(int page, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreateTime);
            
        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (products, totalCount);
    }
    
    /// <summary>
    /// 根据分类ID获取商品
    /// </summary>
    public async Task<List<Product>> GetProductsByCategoryIdAsync(Guid categoryId)
    {
        return await _context.Products
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
            .OrderByDescending(p => p.CreateTime)
            .ToListAsync();
    }
    
    /// <summary>
    /// 分页根据分类ID获取商品
    /// </summary>
    public async Task<(List<Product> Products, int TotalCount)> GetProductsByCategoryIdPagedAsync(Guid categoryId, int page, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
            .OrderByDescending(p => p.CreateTime);
            
        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (products, totalCount);
    }
    
    /// <summary>
    /// 搜索商品
    /// </summary>
    public async Task<List<Product>> SearchProductsAsync(string keyword)
    {
        return await _context.Products
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted && 
                       (p.Name.Contains(keyword) || p.Description.Contains(keyword)))
            .OrderByDescending(p => p.CreateTime)
            .ToListAsync();
    }
    
    /// <summary>
    /// 分页搜索商品
    /// </summary>
    public async Task<(List<Product> Products, int TotalCount)> SearchProductsPagedAsync(string keyword, int page, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted && 
                       (p.Name.Contains(keyword) || p.Description.Contains(keyword)))
            .OrderByDescending(p => p.CreateTime);
            
        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (products, totalCount);
    }
    
    /// <summary>
    /// 分页根据分类ID和搜索条件获取商品
    /// </summary>
    public async Task<(List<Product> Products, int TotalCount)> GetProductsByCategoryIdAndSearchPagedAsync(Guid categoryId, string keyword, int page, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted && 
                       p.CategoryId == categoryId &&
                       (p.Name.Contains(keyword) || p.Description.Contains(keyword)))
            .OrderByDescending(p => p.CreateTime);
            
        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (products, totalCount);
    }
    
    /// <summary>
    /// 更新商品信息
    /// </summary>
    public async Task<Product> UpdateProductAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }
    
    /// <summary>
    /// 删除商品（软删除）
    /// </summary>
    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }
        
        product.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 检查商品是否存在
    /// </summary>
    public async Task<bool> ProductExistsAsync(Guid id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id && !p.IsDeleted);
    }
    

    
    /// <summary>
    /// 获取商品数量
    /// </summary>
    public async Task<int> GetProductCountAsync()
    {
        return await _context.Products.CountAsync(p => !p.IsDeleted);
    }
    
    /// <summary>
    /// 获取用户商品数量
    /// </summary>
    public async Task<int> GetUserProductCountAsync(long userId)
    {
        return await _context.Products.CountAsync(p => p.UploaderUserId == userId && !p.IsDeleted);
    }
    
    /// <summary>
    /// 根据分类ID获取商品平均价格
    /// </summary>
    public async Task<decimal?> GetAveragePriceByCategoryAsync(Guid categoryId)
    {
        // 验证分类是否存在
        var categoryExists = await _context.ProductCategories.AnyAsync(c => c.Id == categoryId);
        if (!categoryExists)
        {
            throw new Exception("Category not found");
        }
        
        // 计算该分类下所有未删除商品的平均价格
        return await _context.Products
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
            .AverageAsync(p => (decimal?)p.Price);
    }
}