using Microsoft.AspNetCore.Mvc;
using SouthernMoneyBackend.Utils;
using SouthernMoneyBackend.Middleware;
using Service;
using Database;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/transaction")]
[AuthorizeUser]
public class TransactionController : ControllerBase
{
    private readonly TransactionService _transactionService;
    private readonly ProductService _productService;
    private readonly UserService _userService;
    
    public TransactionController(
        TransactionService transactionService,
        ProductService productService,
        UserService userService)
    {
        _transactionService = transactionService;
        _productService = productService;
        _userService = userService;
    }

    // POST /transaction/buy
    [HttpPost("buy")]
    public async Task<ApiResponse<object>> BuyProduct([FromBody] BuyProductRequest request)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            
            // 验证商品是否存在
            var product = await _productService.GetProductByIdAsync(request.ProductId);
            if (product == null)
            {
                return ApiResponse<object>.Fail("Product not found");
            }
            
            // 验证商品是否已被删除
            if (product.IsDeleted)
            {
                return ApiResponse<object>.Fail("Product has been deleted");
            }
            
            // 验证用户是否是商品的所有者
            if (product.UploaderUserId == userId)
            {
                return ApiResponse<object>.Fail("You cannot buy your own product");
            }
            
            // 创建交易记录（默认购买数量为1）
            var transaction = await _transactionService.CreateTransactionAsync(request.ProductId, 1, userId);
            
            return ApiResponse<object>.Ok(new ProductPurchaseResultDto { 
                Message = "Product purchased successfully", 
                TransactionId = transaction.Id,
                ProductName = product.Name,
                Price = product.Price,
                PurchaseTime = transaction.PurchaseTime
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail(ex.Message);
        }
    }

    // GET /transaction/myRecords?page={page}&pageSize={pageSize}
    [HttpGet("myRecords")]
    public async Task<ApiResponse<PaginatedResponse<PurchaseRecordDto>>> GetMyRecords(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            
            // 验证分页参数
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;
            
            // 获取用户购买记录（分页）
            var (transactions, totalCount) = await _transactionService.GetUserPurchaseHistoryPagedAsync(userId, page, pageSize);
            
            // 转换为DTO
        var purchaseRecords = PurchaseRecordDto.FromTransactionRecordList(transactions);
            
            // 创建分页响应
            var paginatedResponse = PaginatedResponse<PurchaseRecordDto>.Create(purchaseRecords, page, pageSize, totalCount);
            return ApiResponse<PaginatedResponse<PurchaseRecordDto>>.Ok(paginatedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResponse<PurchaseRecordDto>>.Fail(ex.Message);
        }
    }
}
