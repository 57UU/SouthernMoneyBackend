using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

/// <summary>
/// 交易记录数据访问层
/// </summary>
public class TransactionRepository
{
    private readonly AppDbContext _context;
    
    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 添加新交易记录（不自动保存）
    /// </summary>
    public async Task<TransactionRecord> AddTransactionAsync(TransactionRecord transaction, bool saveChanges = true)
    {
        _context.TransactionRecords.Add(transaction);
        
        if (saveChanges)
        {
            await _context.SaveChangesAsync();
        }
        
        return transaction;
    }
    
    /// <summary>
    /// 根据ID获取交易记录
    /// </summary>
    public async Task<TransactionRecord?> GetTransactionByIdAsync(Guid id)
    {
        return await _context.TransactionRecords
            .Include(t => t.Product)
            .Include(t => t.Buyer)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
    
    /// <summary>
    /// 获取用户的所有购买记录
    /// </summary>
    public async Task<List<TransactionRecord>> GetTransactionsByBuyerIdAsync(long buyerId)
    {
        return await _context.TransactionRecords
            .Include(t => t.Product)
            .Where(t => t.BuyerUserId == buyerId)
            .OrderByDescending(t => t.PurchaseTime)
            .ToListAsync();
    }
    
    /// <summary>
    /// 分页获取用户的购买记录
    /// </summary>
    public async Task<(List<TransactionRecord> Transactions, int TotalCount)> GetTransactionsByBuyerIdPagedAsync(long buyerId, int page, int pageSize)
    {
        var query = _context.TransactionRecords
            .Include(t => t.Product)
            .Where(t => t.BuyerUserId == buyerId)
            .OrderByDescending(t => t.PurchaseTime);
            
        var totalCount = await query.CountAsync();
        var transactions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (transactions, totalCount);
    }
    
    /// <summary>
    /// 获取商品的所有交易记录
    /// </summary>
    public async Task<List<TransactionRecord>> GetTransactionsByProductIdAsync(Guid productId)
    {
        return await _context.TransactionRecords
            .Include(t => t.Buyer)
            .Where(t => t.ProductId == productId)
            .OrderByDescending(t => t.PurchaseTime)
            .ToListAsync();
    }
    
    /// <summary>
    /// 获取用户的所有销售记录
    /// </summary>
    public async Task<List<TransactionRecord>> GetTransactionsBySellerIdAsync(long sellerId)
    {
        return await _context.TransactionRecords
            .Include(t => t.Product)
            .Include(t => t.Buyer)
            .Where(t => t.Product.UploaderUserId == sellerId)
            .OrderByDescending(t => t.PurchaseTime)
            .ToListAsync();
    }
    
    /// <summary>
    /// 获取所有交易记录
    /// </summary>
    public async Task<List<TransactionRecord>> GetAllTransactionsAsync()
    {
        return await _context.TransactionRecords
            .Include(t => t.Product)
            .Include(t => t.Buyer)
            .OrderByDescending(t => t.PurchaseTime)
            .ToListAsync();
    }
    
    /// <summary>
    /// 分页获取交易记录
    /// </summary>
    public async Task<(List<TransactionRecord> Transactions, int TotalCount)> GetTransactionsPagedAsync(int page, int pageSize)
    {
        var query = _context.TransactionRecords
            .Include(t => t.Product)
            .Include(t => t.Buyer)
            .OrderByDescending(t => t.PurchaseTime);
            
        var totalCount = await query.CountAsync();
        var transactions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (transactions, totalCount);
    }
    
    /// <summary>
    /// 更新交易记录
    /// </summary>
    public async Task<TransactionRecord> UpdateTransactionAsync(TransactionRecord transaction)
    {
        _context.TransactionRecords.Update(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }
    
    /// <summary>
    /// 检查交易记录是否存在
    /// </summary>
    public async Task<bool> TransactionExistsAsync(Guid id)
    {
        return await _context.TransactionRecords.AnyAsync(t => t.Id == id);
    }
    
    /// <summary>
    /// 获取用户购买的商品数量
    /// </summary>
    public async Task<int> GetUserPurchaseCountAsync(long buyerId)
    {
        return await _context.TransactionRecords
            .CountAsync(t => t.BuyerUserId == buyerId);
    }
    
    /// <summary>
    /// 获取用户销售的商品数量
    /// </summary>
    public async Task<int> GetUserSaleCountAsync(long sellerId)
    {
        return await _context.TransactionRecords
            .CountAsync(t => t.Product.UploaderUserId == sellerId);
    }
    
    /// <summary>
    /// 获取用户总销售额
    /// </summary>
    public async Task<decimal> GetUserTotalSalesAsync(long sellerId)
    {
        return await _context.TransactionRecords
            .Where(t => t.Product.UploaderUserId == sellerId)
            .SumAsync(t => t.TotalPrice);
    }
    
    /// <summary>
    /// 获取用户总消费额
    /// </summary>
    public async Task<decimal> GetUserTotalSpendingAsync(long buyerId)
    {
        return await _context.TransactionRecords
            .Where(t => t.BuyerUserId == buyerId)
            .SumAsync(t => t.TotalPrice);
    }
    
    /// <summary>
    /// 获取指定日期范围内的交易记录
    /// </summary>
    public async Task<List<TransactionRecord>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.TransactionRecords
            .Include(t => t.Product)
            .Include(t => t.Buyer)
            .Where(t => t.PurchaseTime >= startDate && t.PurchaseTime <= endDate)
            .OrderByDescending(t => t.PurchaseTime)
            .ToListAsync();
    }
    
    /// <summary>
    /// 获取总交易数量
    /// </summary>
    public async Task<int> GetTransactionCountAsync()
    {
        return await _context.TransactionRecords.CountAsync();
    }
    
    /// <summary>
    /// 获取最近N天的交易数量
    /// </summary>
    public async Task<int> GetRecentTransactionCountAsync(int days)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        return await _context.TransactionRecords
            .CountAsync(t => t.PurchaseTime >= startDate);
    }

    /// <summary>
    /// 获取指定日期之后的交易数量
    /// </summary>
    public async Task<int> GetTransactionCountSinceDateAsync(DateTime date)
    {
        return await _context.TransactionRecords
            .CountAsync(t => t.PurchaseTime >= date);
    }
}