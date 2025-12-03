//following is written by hr
using Microsoft.AspNetCore.Mvc;
using SouthernMoneyBackend.Utils;
using SouthernMoneyBackend.Middleware;
using Service;
using Database;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/store")]
public class StoreController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly ProductCategoryService _categoryService;
    private readonly UserService _userService;
    private readonly UserFavoriteCategoryService _favoriteCategoryService;
    
    public StoreController(
        ProductService productService,
        ProductCategoryService categoryService,
        UserService userService,
        UserFavoriteCategoryService favoriteCategoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _userService = userService;
        _favoriteCategoryService = favoriteCategoryService;
    }

    // GET /store/myProducts
    [HttpGet("myProducts")]
    [AuthorizeUser]
    public async Task<ApiResponse<PaginatedResponse<ProductDto>>> GetMyProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // 从HttpContext中获取用户ID
            if (!HttpContext.Items.ContainsKey("UserId") || HttpContext.Items["UserId"] == null)
            {
                return ApiResponse<PaginatedResponse<ProductDto>>.Fail("User not authenticated");
            }
            
            if (HttpContext.Items["UserId"] is not long userId)
            {
                return ApiResponse<PaginatedResponse<ProductDto>>.Fail("Invalid user ID");
            }
            
            // 验证分页参数
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;
            
            // 获取用户发布的商品
            var result = await _productService.GetUserProductsPagedAsync(userId, page, pageSize);
            var products = result.Products;
            var totalCount = result.TotalCount;
            
            // 使用ProductDto的工厂方法转换为DTO
            var productDtos = ProductDto.FromProductList(products);
            
            var paginatedResponse = PaginatedResponse<ProductDto>.Create(
                productDtos, 
                page, 
                pageSize, 
                totalCount);
                
            return ApiResponse<PaginatedResponse<ProductDto>>.Ok(paginatedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<ProductDto>>.Fail(ex.Message);
        }
    }

    // GET /store/products
    [HttpGet("products")]
    public async Task<ApiResponse<PaginatedResponse<ProductDto>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null)
    {
        try
        {
            // 验证分页参数
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;
            
            List<Product> products;
            int totalCount;
            
            // 根据条件获取商品
            if (categoryId.HasValue && !string.IsNullOrWhiteSpace(search))
            {
                // 同时有分类ID和搜索条件，使用Service层的组合查询方法
                var result = await _productService.GetProductsByCategoryAndSearchPagedAsync(categoryId.Value, search, page, pageSize);
                products = result.Products;
                totalCount = result.TotalCount;
            }
            else if (categoryId.HasValue)
            {
                // 只有分类ID条件，使用Service层的分页方法
                var result = await _productService.GetProductsByCategoryPagedAsync(categoryId.Value, page, pageSize);
                products = result.Products;
                totalCount = result.TotalCount;
            }
            else if (!string.IsNullOrWhiteSpace(search))
            {
                // 只有搜索条件，使用Service层的分页搜索方法
                var result = await _productService.SearchProductsPagedAsync(search, page, pageSize);
                products = result.Products;
                totalCount = result.TotalCount;
            }
            else
            {
                // 无条件，获取所有商品
                var result = await _productService.GetProductsPagedAsync(page, pageSize);
                products = result.Products;
                totalCount = result.TotalCount;
            }
            
            // 使用ProductDto的工厂方法转换为DTO
            var productDtos = ProductDto.FromProductList(products);
            
            var paginatedResponse = PaginatedResponse<ProductDto>.Create(
                productDtos, 
                totalCount, 
                page, 
                pageSize);
                
            return ApiResponse<PaginatedResponse<ProductDto>>.Ok(paginatedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<ProductDto>>.Fail(ex.Message);
        }
    }

    // GET /store/products/{id}
    [HttpGet("products/{id}")]
    public async Task<ApiResponse<ProductDto>> GetProduct(Guid id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return ApiResponse<ProductDto>.Fail("Product not found");
            }
            
            // 使用ProductDto的工厂方法转换为DTO
            var productDto = ProductDto.FromProduct(product);
            
            return ApiResponse<ProductDto>.Ok(productDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductDto>.Fail(ex.Message);
        }
    }
    
    // GET /store/detail?id={productId} - for compatibility with README
    [HttpGet("detail")]
    public async Task<ApiResponse<ProductDto>> GetProductDetail([FromQuery(Name = "id")] Guid productId)
    {
        // 复用现有逻辑
        return await GetProduct(productId);
    }

    // GET /store/categories
    [HttpGet("categories")]
    public async Task<ApiResponse<List<ProductCategoryDto>>> GetCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            
            var categoryDtos = categories.Select(category => new ProductCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                CoverImageId = category.CoverImageId,
                CreatedAt = category.CreatedAt
            }).ToList();
            
            return ApiResponse<List<ProductCategoryDto>>.Ok(categoryDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ProductCategoryDto>>.Fail(ex.Message);
        }
    }

    // GET /store/categories/{id}
    [HttpGet("categories/{id}")]
    public async Task<ApiResponse<ProductCategoryDto>> GetCategory(Guid id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return ApiResponse<ProductCategoryDto>.Fail("Category not found");
            }
            
            var categoryDto = new ProductCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                CoverImageId = category.CoverImageId,
                CreatedAt = category.CreatedAt
            };
            
            return ApiResponse<ProductCategoryDto>.Ok(categoryDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductCategoryDto>.Fail(ex.Message);
        }
    }
    
    // GET /store/category/search?name={name} - for compatibility with README
    [HttpGet("category/search")]
    public async Task<ApiResponse<CategorySearchResultDto>> SearchCategories([FromQuery(Name = "name")] string name)
    {
        try
        {
            // 使用服务层搜索分类
            var categories = await _categoryService.SearchCategoriesAsync(name);
            
            // 只返回最多10个分类，并提取名称列表
            var categoryNames = categories
                .Take(10)
                .Select(c => c.Name)
                .ToList();
            
            // 返回符合README格式的响应
            return ApiResponse<CategorySearchResultDto>.Ok(new CategorySearchResultDto(categoryNames));
        }
        catch (Exception ex)
        {
            return ApiResponse<CategorySearchResultDto>.Fail(ex.Message);
        }
    }
    
    // POST /store/category/create - for compatibility with README
    [HttpPost("category/create")]
    [AuthorizeUser]
    public async Task<ApiResponse> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            // 验证必填参数
            if (string.IsNullOrWhiteSpace(request.Category))
            {
                return ApiResponse.Fail("Category name is required");
            }
            
            if (string.IsNullOrWhiteSpace(request.Cover))
            {
                return ApiResponse.Fail("Cover image ID is required");
            }
            
            // 解析封面图片ID
            if (!Guid.TryParse(request.Cover, out var coverImageId))
            {
                return ApiResponse.Fail("Invalid cover image ID format");
            }
            
            // 调用服务层创建分类
            await _categoryService.CreateCategoryAsync(request.Category, coverImageId);
            
            return ApiResponse.Ok();
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail(ex.Message);
        }
    }

    // GET /store/categories/{id}/products
    [HttpGet("categories/{id}/products")]
    public async Task<ApiResponse<PaginatedResponse<ProductDto>>> GetProductsByCategory(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // 验证分页参数
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;
            
            // 直接使用Service层的分类ID分页方法获取分类下的商品
            var result = await _productService.GetProductsByCategoryIdPagedAsync(id, page, pageSize);
            var products = result.Products;
            var totalCount = result.TotalCount;
            
            // 使用ProductDto的工厂方法转换为DTO
            var productDtos = ProductDto.FromProductList(products);
            
            var paginatedResponse = PaginatedResponse<ProductDto>.Create(
                productDtos, 
                totalCount, 
                page, 
                pageSize);
                
            return ApiResponse<PaginatedResponse<ProductDto>>.Ok(paginatedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<ProductDto>>.Fail(ex.Message);
        }
    }

    // GET /store/search
    [HttpGet("search")]
    public async Task<ApiResponse<PaginatedResponse<ProductDto>>> SearchProducts(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // 验证搜索词
            if (string.IsNullOrWhiteSpace(q))
            {
                return ApiResponse<PaginatedResponse<ProductDto>>.Fail("Search query is required");
            }
            
            // 使用Service层的分页搜索功能
            var searchResult = await _productService.SearchProductsPagedAsync(q, page, pageSize);
            var products = searchResult.Products;
            var totalCount = searchResult.TotalCount;
            
            // 使用ProductDto的工厂方法转换为DTO
            var productDtos = ProductDto.FromProductList(products);
            
            var paginatedResponse = PaginatedResponse<ProductDto>.Create(
                productDtos, 
                totalCount, 
                page, 
                pageSize);
                
            return ApiResponse<PaginatedResponse<ProductDto>>.Ok(paginatedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<ProductDto>>.Fail(ex.Message);
        }
    }
    
    // GET /store/avgPrice
    [HttpGet("avgPrice")]
    public async Task<ApiResponse<object>> GetAveragePriceByCategory([FromQuery] Guid categoryId)
    {
        try
        {
            // 获取分类平均价格
            var avgPrice = await _productService.GetAveragePriceByCategoryAsync(categoryId);
            
            // 返回符合API规范的结果
            var result = new { AvgPrice = avgPrice };
            
            return ApiResponse<object>.Ok(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail(ex.Message);
        }
    }
    
    // POST /store/publish
    [HttpPost("publish")]
    [AuthorizeUser]
    public async Task<ApiResponse<object>> PublishProduct([FromBody] PublishProductRequest request)
    {
        try
        {
            // 从HttpContext中获取用户ID
            if (HttpContext.Items["UserId"] is not long userId)
            {
                return ApiResponse<object>.Fail("User not authenticated");
            }
            
            // 验证必填参数
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return ApiResponse<object>.Fail("Product name is required");
            }
            
            if (request.Price <= 0)
            {
                return ApiResponse<object>.Fail("Product price must be positive");
            }
            
            // 调用服务层创建商品
            var product = await _productService.CreateProductAsync(
                request.Name,
                request.Price,
                request.Description,
                request.CategoryId,
                userId);
            
            return ApiResponse<object>.Ok(new { ProductId = product.Id });
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail(ex.Message);
        }
    }
    
    // POST /store/delete
    [HttpPost("delete")]
    [AuthorizeUser]
    public async Task<ApiResponse> DeleteProduct([FromBody] DeleteProductRequest request)
    {
        try
        {
            // 从HttpContext中获取用户ID
            if (HttpContext.Items["UserId"] is not long userId)
            {
                return ApiResponse.Fail("User not authenticated");
            }
            
            // 调用服务层删除商品
            var success = await _productService.DeleteProductAsync(request.ProductId, userId);
            if (!success)
            {
                return ApiResponse.Fail("Product not found or you don't have permission to delete it");
            }
            
            return ApiResponse.Ok();
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail(ex.Message);
        }
    }
    
    // POST /store/categories/{categoryId}/favorite
    [HttpPost("categories/{categoryId}/favorite")]
    [AuthorizeUser]
    public async Task<ApiResponse> FavoriteCategory(Guid categoryId)
    {
        try
        {
            // 从HttpContext中获取用户ID
            if (HttpContext.Items["UserId"] is not long userId)
            {
                return ApiResponse.Fail("User not authenticated");
            }
            
            // 添加收藏
            await _favoriteCategoryService.AddFavoriteCategoryAsync(userId, categoryId);
            
            return ApiResponse.Ok();
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail(ex.Message);
        }
    }
    
    // POST /store/categories/{categoryId}/unfavorite
    [HttpPost("categories/{categoryId}/unfavorite")]
    [AuthorizeUser]
    public async Task<ApiResponse> UnfavoriteCategory(Guid categoryId)
    {
        try
        {
            // 从HttpContext中获取用户ID
            if (HttpContext.Items["UserId"] is not long userId)
            {
                return ApiResponse.Fail("User not authenticated");
            }
            
            // 取消收藏
            var success = await _favoriteCategoryService.RemoveFavoriteCategoryAsync(userId, categoryId);
            if (!success)
            {
                return ApiResponse.Fail("Category not favorited");
            }
            
            return ApiResponse.Ok();
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail(ex.Message);
        }
    }
    
    // GET /store/favoriteCategories
    [HttpGet("favoriteCategories")]
    [AuthorizeUser]
    public async Task<ApiResponse<List<ProductCategoryDto>>> GetFavoriteCategories()
    {
        try
        {
            // 从HttpContext中获取用户ID
            if (HttpContext.Items["UserId"] is not long userId)
            {
                return ApiResponse<List<ProductCategoryDto>>.Fail("User not authenticated");
            }
            
            // 获取用户收藏分类
            var categories = await _favoriteCategoryService.GetUserFavoriteCategoriesAsync(userId);
            
            // 转换为DTO
            var categoryDtos = categories.Select(category => new ProductCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                CoverImageId = category.CoverImageId,
                CreatedAt = category.CreatedAt
            }).ToList();
            
            return ApiResponse<List<ProductCategoryDto>>.Ok(categoryDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ProductCategoryDto>>.Fail(ex.Message);
        }
    }
    
    // GET /store/categories/{categoryId}/isFavorited
    [HttpGet("categories/{categoryId}/isFavorited")]
    [AuthorizeUser]
    public async Task<ApiResponse<object>> IsCategoryFavorited(Guid categoryId)
    {
        try
        {
            // 从HttpContext中获取用户ID
            if (HttpContext.Items["UserId"] is not long userId)
            {
                return ApiResponse<object>.Fail("User not authenticated");
            }
            
            // 检查是否已收藏
            var isFavorited = await _favoriteCategoryService.IsCategoryFavoritedAsync(userId, categoryId);
            
            return ApiResponse<object>.Ok(new { IsFavorited = isFavorited });
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail(ex.Message);
        }
    }
}