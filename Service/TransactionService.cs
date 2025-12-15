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
    private readonly AppDbContext _context;
    
    public TransactionService(
        TransactionRepository transactionRepository,
        ProductRepository productRepository,
        UserRepository userRepository,
        UserAssetRepository userAssetRepository,
        AppDbContext context)
    {
        _transactionRepository = transactionRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _userAssetRepository = userAssetRepository;
        _context = context;
    }
    
    /// <summary>
    /// 创建交易记录
    /// </summary>
    public async Task<TransactionRecord> CreateTransactionAsync(Guid productId, int quantity, long buyerId)
    {
        // 使用事务包裹所有操作
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
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
            
            // 检查销售者资产是否存在
            var sellerAsset = await _userAssetRepository.GetUserAssetByUserIdAsync(product.UploaderUserId);
            if (sellerAsset == null)
            {
                throw new Exception("Seller asset not found");
            }
            
            // 扣除购买者余额（不自动保存）
            var subtractResult = await _userAssetRepository.SubtractFromUserBalanceAsync(buyerId, totalPrice, false);
            if (!subtractResult)
            {
                throw new Exception("Failed to subtract balance from buyer");
            }
            
            // 增加销售者余额（不自动保存）
            var addResult = await _userAssetRepository.AddToUserBalanceAsync(product.UploaderUserId, totalPrice, false);
            if (!addResult)
            {
                throw new Exception("Failed to add balance to seller");
            }
            
            // 更新销售者收益（不自动保存）
            sellerAsset.AccumulatedEarn += totalPrice;
            sellerAsset.TodayEarn += totalPrice;
            sellerAsset.Total = sellerAsset.Balance; // 确保总资产等于余额
            await _userAssetRepository.UpdateUserAssetAsync(sellerAsset, false);
            
            // 将商品标记为已删除（不自动保存）
            product.IsDeleted = true;
            await _productRepository.UpdateProductAsync(product, false);
            
            // 创建交易记录（不自动保存）
            var transactionRecord = new TransactionRecord
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                BuyerUserId = buyerId,
                Quantity = quantity,
                Price = product.Price,
                TotalPrice = totalPrice,
                PurchaseTime = DateTime.UtcNow
            };
            
            await _transactionRepository.AddTransactionAsync(transactionRecord, false);
            
            // 一次性保存所有更改
            await _context.SaveChangesAsync();
            
            // 提交事务
            await transaction.CommitAsync();
            
            return transactionRecord;
        }
        catch (Exception ex)
        {
            // 回滚事务
            await transaction.RollbackAsync();
            throw;
        }
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
