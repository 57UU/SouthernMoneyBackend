using Microsoft.EntityFrameworkCore;
using Database.Repositories;
using Database;

namespace Service;

/// <summary>
/// 交易记录服务层
/// </summary>
public class TransactionService
{
    private readonly TransactionRepository _transactionRepository;
    private readonly ProductRepository _productRepository;
    private readonly UserRepository _userRepository;
    private readonly UserAssetRepository _userAssetRepository;
    
    public TransactionService(
        TransactionRepository transactionRepository,
        ProductRepository productRepository,
        UserRepository userRepository,
        UserAssetRepository userAssetRepository)
    {
        _transactionRepository = transactionRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _userAssetRepository = userAssetRepository;
    }
    
    /// <summary>
    /// 创建交易记录
    /// </summary>
    public async Task<TransactionRecord> CreateTransactionAsync(Guid productId, int quantity, long buyerId)
    {
        // 验证商品是否存在
        var product = await _productRepository.GetProductByIdAsync(productId);
        if (product == null)
        {
            throw new Exception("Product not found");
        }
        
        // 验证购买者不是商品的所有者
        if (product.UploaderUserId == buyerId)
        {
            throw new Exception("You cannot buy your own product");
        }
        
        // 计算总价
        var totalPrice = product.Price * quantity;
        
        // 检查购买者余额是否足够
        var buyerAsset = await _userAssetRepository.GetUserAssetByUserIdAsync(buyerId);
        if (buyerAsset == null || buyerAsset.Balance < totalPrice)
        {
            throw new Exception("Insufficient balance");
        }
        
        // 扣除购买者余额
        await _userAssetRepository.SubtractFromUserBalanceAsync(buyerId, totalPrice);
        
        // 增加销售者余额
        var sellerAsset = await _userAssetRepository.GetUserAssetByUserIdAsync(product.UploaderUserId);
        if (sellerAsset != null)
        {
            await _userAssetRepository.AddToUserBalanceAsync(product.UploaderUserId, totalPrice);
            
            // 更新销售者收益
            sellerAsset.AccumulatedEarn += totalPrice;
            sellerAsset.TodayEarn += totalPrice;
            await _userAssetRepository.UpdateUserAssetAsync(sellerAsset);
        }
        
        // 将商品标记为已删除
        product.IsDeleted = true;
        await _productRepository.UpdateProductAsync(product);
        
        // 创建交易记录
        var transaction = new TransactionRecord
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            BuyerUserId = buyerId,
            Quantity = quantity,
            Price = product.Price,
            TotalPrice = totalPrice,
            PurchaseTime = DateTime.UtcNow
        };
        
        return await _transactionRepository.AddTransactionAsync(transaction);
    }
    
    /// <summary>
    /// 获取交易记录详情
    /// </summary>
    public async Task<TransactionRecord?> GetTransactionByIdAsync(Guid id)
    {
        return await _transactionRepository.GetTransactionByIdAsync(id);
    }
    
    /// <summary>
    /// 获取用户的所有购买记录
    /// </summary>
    public async Task<List<TransactionRecord>> GetUserPurchaseHistoryAsync(long userId)
    {
        return await _transactionRepository.GetTransactionsByBuyerIdAsync(userId);
    }
    
    /// <summary>
    /// 分页获取用户的购买记录
    /// </summary>
    public async Task<(List<TransactionRecord> Transactions, int TotalCount)> GetUserPurchaseHistoryPagedAsync(long userId, int page, int pageSize)
    {
        return await _transactionRepository.GetTransactionsByBuyerIdPagedAsync(userId, page, pageSize);
    }
    
    /// <summary>
    /// 获取用户的所有销售记录
    /// </summary>
    public async Task<List<TransactionRecord>> GetUserSalesHistoryAsync(long userId)
    {
        return await _transactionRepository.GetTransactionsBySellerIdAsync(userId);
    }
    
    /// <summary>
    /// 获取商品的所有交易记录
    /// </summary>
    public async Task<List<TransactionRecord>> GetProductTransactionHistoryAsync(Guid productId)
    {
        return await _transactionRepository.GetTransactionsByProductIdAsync(productId);
    }
    
    /// <summary>
    /// 获取所有交易记录
    /// </summary>
    public async Task<List<TransactionRecord>> GetAllTransactionsAsync()
    {
        return await _transactionRepository.GetAllTransactionsAsync();
    }
    
    /// <summary>
    /// 分页获取交易记录
    /// </summary>
    public async Task<(List<TransactionRecord> Transactions, int TotalCount)> GetTransactionsPagedAsync(int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        return await _transactionRepository.GetTransactionsPagedAsync(page, pageSize);
    }
    
    /// <summary>
    /// 获取用户总销售额
    /// </summary>
    public async Task<decimal> GetUserTotalSalesAsync(long userId)
    {
        return await _transactionRepository.GetUserTotalSalesAsync(userId);
    }
    
    /// <summary>
    /// 获取用户总消费额
    /// </summary>
    public async Task<decimal> GetUserTotalSpendingAsync(long userId)
    {
        return await _transactionRepository.GetUserTotalSpendingAsync(userId);
    }
    
    /// <summary>
    /// 获取用户购买的商品数量
    /// </summary>
    public async Task<int> GetUserPurchaseCountAsync(long userId)
    {
        return await _transactionRepository.GetUserPurchaseCountAsync(userId);
    }
    
    /// <summary>
    /// 获取用户销售的商品数量
    /// </summary>
    public async Task<int> GetUserSaleCountAsync(long userId)
    {
        return await _transactionRepository.GetUserSaleCountAsync(userId);
    }
    
    /// <summary>
    /// 获取指定日期范围内的交易记录
    /// </summary>
    public async Task<List<TransactionRecord>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            throw new Exception("Start date cannot be later than end date");
        }
        
        return await _transactionRepository.GetTransactionsByDateRangeAsync(startDate, endDate);
    }
}
