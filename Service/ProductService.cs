using Microsoft.EntityFrameworkCore;
using Database.Repositories;
using Database;

namespace Service;

/// <summary>
/// 商品服务层
/// </summary>
public class ProductService
{
    private readonly ProductRepository _productRepository;
    private readonly UserRepository _userRepository;
    private readonly ProductCategoryRepository _categoryRepository;
    
    public ProductService(
        ProductRepository productRepository,
        UserRepository userRepository,
        ProductCategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
    }


    
    /// <summary>
    /// 创建新商品
    /// </summary>
    public async Task<Product> CreateProductAsync(string name, decimal price, string description, Guid categoryId, long userId)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        // 验证分类是否存在
        var categoryExists = await _categoryRepository.CategoryExistsAsync(categoryId);
        if (!categoryExists)
        {
            throw new Exception("Category not found");
        }
        
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            Description = description,
            CategoryId = categoryId,
            UploaderUserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        return await _productRepository.AddProductAsync(product);
    }
    
    /// <summary>
    /// 获取商品详情
    /// </summary>
    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await _productRepository.GetProductByIdAsync(id);
    }
    
    /// <summary>
    /// 获取用户的所有商品
    /// </summary>
    public async Task<List<Product>> GetUserProductsAsync(long userId)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        return await _productRepository.GetProductsByUserIdAsync(userId);
    }
    
    /// <summary>
    /// 分页获取用户的商品
    /// </summary>
    public async Task<(List<Product> Products, int TotalCount)> GetUserProductsPagedAsync(long userId, int page, int pageSize)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        return await _productRepository.GetProductsByUserIdPagedAsync(userId, page, pageSize);
    }
    
    /// <summary>
    /// 获取所有商品
    /// </summary>
    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _productRepository.GetAllProductsAsync();
    }
    
    /// <summary>
    /// 分页获取商品
    /// </summary>
    public async Task<(List<Product> Products, int TotalCount)> GetProductsPagedAsync(int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        return await _productRepository.GetProductsPagedAsync(page, pageSize);
    }
    
    /// <summary>
    /// 根据分类ID获取商品
    /// </summary>
    public async Task<List<Product>> GetProductsByCategoryAsync(Guid categoryId)
    {
        // 验证分类是否存在
        var categoryExists = await _categoryRepository.CategoryExistsAsync(categoryId);
        if (!categoryExists)
        {
            throw new Exception("Category not found");
        }
        
        return await _productRepository.GetProductsByCategoryIdAsync(categoryId);
    }
    
    /// <summary>
    /// 分页根据分类ID获取商品
    /// </summary>
    public async Task<(List<Product> Products, int TotalCount)> GetProductsByCategoryPagedAsync(Guid categoryId, int page, int pageSize)
    {
        // 验证分类是否存在
        var categoryExists = await _categoryRepository.CategoryExistsAsync(categoryId);
        if (!categoryExists)
        {
            throw new Exception("Category not found");
        }
        
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        return await _productRepository.GetProductsByCategoryIdPagedAsync(categoryId, page, pageSize);
    }
    
    /// <summary>
    /// 搜索商品
    /// </summary>
    public async Task<List<Product>> SearchProductsAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<Product>();
        }
        
        return await _productRepository.SearchProductsAsync(keyword);
    }
    
    /// <summary>
    /// 分页搜索商品
    /// </summary>
    public async Task<(List<Product> Products, int TotalCount)> SearchProductsPagedAsync(string keyword, int page, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return (new List<Product>(), 0);
        }
        
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        return await _productRepository.SearchProductsPagedAsync(keyword, page, pageSize);
    }
    
    /// <summary>
    /// 分页根据分类ID和搜索条件获取商品
    /// </summary>
    public async Task<(List<Product> Products, int TotalCount)> GetProductsByCategoryAndSearchPagedAsync(Guid categoryId, string keyword, int page, int pageSize)
    {
        // 验证分类是否存在
        var categoryExists = await _categoryRepository.CategoryExistsAsync(categoryId);
        if (!categoryExists)
        {
            throw new Exception("Category not found");
        }
        
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return (new List<Product>(), 0);
        }
        
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        return await _productRepository.GetProductsByCategoryIdAndSearchPagedAsync(categoryId, keyword, page, pageSize);
    }
    
    /// <summary>
    /// 分页根据分类ID获取商品
    /// </summary>
    public async Task<(List<Product> Products, int TotalCount)> GetProductsByCategoryIdPagedAsync(Guid categoryId, int page, int pageSize)
    {
        // 验证分类是否存在
        var categoryExists = await _categoryRepository.CategoryExistsAsync(categoryId);
        if (!categoryExists)
        {
            throw new Exception("Category not found");
        }
        
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        return await _productRepository.GetProductsByCategoryIdPagedAsync(categoryId, page, pageSize);
    }
    
    /// <summary>
    /// 分页根据分类ID和搜索条件获取商品
    /// </summary>
    public async Task<(List<Product> Products, int TotalCount)> GetProductsByCategoryIdAndSearchPagedAsync(Guid categoryId, string keyword, int page, int pageSize)
    {
        // 验证分类是否存在
        var categoryExists = await _categoryRepository.CategoryExistsAsync(categoryId);
        if (!categoryExists)
        {
            throw new Exception("Category not found");
        }
        
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return (new List<Product>(), 0);
        }
        
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        return await _productRepository.GetProductsByCategoryIdAndSearchPagedAsync(categoryId, keyword, page, pageSize);
    }
    
    /// <summary>
    /// 更新商品信息
    /// </summary>
    public async Task<Product> UpdateProductAsync(Guid id, string name, decimal price, string description, Guid categoryId, long userId)
    {
        // 验证商品是否存在
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
        {
            throw new Exception("Product not found");
        }
        
        // 验证用户是否是商品的所有者
        if (product.UploaderUserId != userId)
        {
            throw new Exception("You are not the owner of this product");
        }
        
        // 验证分类是否存在
        var categoryExists = await _categoryRepository.CategoryExistsAsync(categoryId);
        if (!categoryExists)
        {
            throw new Exception("Category not found");
        }
        
        product.Name = name;
        product.Price = price;
        product.Description = description;
        product.CategoryId = categoryId;
        
        return await _productRepository.UpdateProductAsync(product);
    }
    
    /// <summary>
    /// 删除商品
    /// </summary>
    public async Task<bool> DeleteProductAsync(Guid id, long userId)
    {
        // 验证商品是否存在
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
        {
            throw new Exception("Product not found");
        }
        
        // 验证用户是否是商品的所有者
        if (product.UploaderUserId != userId)
        {
            throw new Exception("You are not the owner of this product");
        }
        
        return await _productRepository.DeleteProductAsync(id);
    }
    
    /// <summary>
    /// 获取商品数量
    /// </summary>
    public async Task<int> GetProductCountAsync()
    {
        return await _productRepository.GetProductCountAsync();
    }
    
    /// <summary>
    /// 获取用户商品数量
    /// </summary>
    public async Task<int> GetUserProductCountAsync(long userId)
    {
        // 验证用户是否存在
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        return await _productRepository.GetUserProductCountAsync(userId);
    }
    
    /// <summary>
    /// 根据分类ID获取商品平均价格
    /// </summary>
    public async Task<decimal> GetAveragePriceByCategoryAsync(Guid categoryId)
    {
        // 验证分类是否存在
        var categoryExists = await _categoryRepository.CategoryExistsAsync(categoryId);
        if (!categoryExists)
        {
            throw new Exception("Category not found");
        }
        
        var avgPrice = await _productRepository.GetAveragePriceByCategoryAsync(categoryId);
        
        // 如果没有该分类的商品，返回0
        return avgPrice ?? 0;
    }
}