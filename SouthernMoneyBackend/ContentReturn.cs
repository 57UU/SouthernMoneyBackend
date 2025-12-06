// this is all by hr
namespace SouthernMoneyBackend;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。

//login response
public class TokenResponseDto
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}

//login
public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreateTime { get; set; }
    public int ReportCount { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsLiked { get; set; }
    public List<PostBlockDto>? PostBlocks { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<Guid> ImageIds { get; set; } = new();
    public PostUploaderDto? Uploader { get; set; }
    
    /// <summary>
    /// 从Post实体创建PostDto的工厂构造函数
    /// </summary>
    public static PostDto FromPost(Database.Post post, bool isLiked)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreateTime = post.CreateTime,
            ReportCount = post.ReportCount,
            ViewCount = post.ViewCount,
            LikeCount = post.LikeCount,
            IsBlocked = post.IsBlocked,
            IsLiked = isLiked,
            PostBlocks = PostBlockDto.FromPostBlockList(post.PostBlocks?.OrderByDescending(pb => pb.ActionTime)),
            Tags = post.PostTags?.Select(t => t.Tag).ToList() ?? new List<string>(),
            ImageIds = post.PostImages?.Select(pi => pi.ImageId).ToList() ?? new List<Guid>(),
            Uploader = post.User == null ? null : new PostUploaderDto
            {
                Id = post.User.Id,
                Name = post.User.Name,
                Avatar= post.User.Avatar
            }
        };
    }
    
    /// <summary>
    /// 从Post实体列表创建PostDto列表
    /// </summary>
    public static List<PostDto> FromPostList(List<Database.Post> posts, Dictionary<Guid, bool> likedPostIds)
    {
        return posts.Select(p => FromPost(p, likedPostIds.ContainsKey(p.Id))).ToList();
    }
}
public class PostBlockDto{
    public DateTime ActionTime { get; set; }
    public bool IsBlock { get; set; }
    public string Reason { get; set; }
    public PostUploaderDto Operator { get; set; }
    
    /// <summary>
    /// 从PostBlock实体创建PostBlockDto的工厂构造函数
    /// </summary>
    public static PostBlockDto FromPostBlock(Database.PostBlock postBlock)
    {
        return new PostBlockDto
        {
            ActionTime = postBlock.ActionTime,
            IsBlock = postBlock.IsBlock,
            Reason = postBlock.Reason,
            Operator = PostUploaderDto.FromUser(postBlock.AdminUser)
        };
    }
    
    /// <summary>
    /// 从PostBlock实体列表创建PostBlockDto列表
    /// </summary>
    public static List<PostBlockDto>? FromPostBlockList(IEnumerable<Database.PostBlock>? postBlocks)
    {
        return postBlocks?.Select(FromPostBlock).ToList();
    }
}
public class PostUploaderDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public Guid Avatar { get; set; }
    public static PostUploaderDto FromUser(Database.User user)
    {
        return new PostUploaderDto
        {
            Id = user.Id,
            Name = user.Name,
            Avatar = user.Avatar
        };
    }
}

public class PostsPageDto
{
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public List<PostDto> Posts { get; set; } = new();
}
public class PostLikeResultDto
{
    public int LikeCount { get; set; }
}

public class PostReportResultDto
{
    public int ReportCount { get; set; }
}
//user
public class UserAssetDto
{
    public decimal Total { get; set; }
    public decimal TodayEarn { get; set; }
    public decimal AccumulatedEarn { get; set; }
    public decimal EarnRate { get; set; }
    public decimal Balance { get; set; }
    
    /// <summary>
    /// 从UserAsset实体创建UserAssetDto的工厂构造函数
    /// </summary>
    public static UserAssetDto FromUserAsset(Database.UserAsset asset)
    {
        return new UserAssetDto
        {
            Total = asset.Total,
            TodayEarn = asset.TodayEarn,
            AccumulatedEarn = asset.AccumulatedEarn,
            EarnRate = asset.EarnRate,
            Balance = asset.Balance
        };
    }
    
    /// <summary>
    /// 创建默认UserAssetDto的工厂构造函数
    /// </summary>
    public static UserAssetDto CreateDefault()
    {
        return new UserAssetDto
        {
            Total = 0,
            TodayEarn = 0,
            AccumulatedEarn = 0,
            EarnRate = 0,
            Balance = 0
        };
    }
}

public class UserProfileDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Guid Avatar { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime CreateTime { get; set; }
    public UserAssetDto Asset { get; set; }
    public bool IsAdmin { get; set; }
    
    /// <summary>
    /// 从User实体和UserAsset实体创建UserProfileDto的工厂构造函数
    /// </summary>
    public static UserProfileDto FromUser(Database.User user, Database.UserAsset asset)
    {
        return FromUser(user, UserAssetDto.FromUserAsset(asset));
    }
    
    /// <summary>
    /// 从User实体和UserAssetDto创建UserProfileDto的工厂构造函数
    /// </summary>
    public static UserProfileDto FromUser(Database.User user, UserAssetDto assetDto)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            Avatar = user.Avatar,
            IsBlocked = user.IsBlocked,
            CreateTime = user.CreateTime,
            Asset = assetDto,
            IsAdmin=user.IsAdmin
        };
    }
}

public class UploadAvatarResultDto
{
    public Guid AvatarId { get; set; }
}

//transaction
public class PurchaseRecordDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime PurchaseTime { get; set; }
    
    /// <summary>
    /// 从TransactionRecord实体创建PurchaseRecordDto的工厂构造函数
    /// </summary>
    public static PurchaseRecordDto FromTransactionRecord(Database.TransactionRecord transaction)
    {
        return new PurchaseRecordDto
        {
            Id = transaction.Id,
            ProductId = transaction.ProductId,
            Quantity = transaction.Quantity,
            Price = transaction.Price,
            TotalPrice = transaction.TotalPrice,
            PurchaseTime = transaction.PurchaseTime
        };
    }
    
    /// <summary>
    /// 从TransactionRecord实体列表创建PurchaseRecordDto列表
    /// </summary>
    public static List<PurchaseRecordDto> FromTransactionRecordList(List<Database.TransactionRecord> transactions)
    {
        return transactions.Select(t => FromTransactionRecord(t)).ToList();
    }
}

public class ProductPurchaseResultDto
{
    public string Message { get; set; }
    public Guid TransactionId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public DateTime PurchaseTime { get; set; }
}

// store
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public long UploaderUserId { get; set; }
    public string UploaderName { get; set; }
    public DateTime CreateTime { get; set; }
    
    /// <summary>
    /// 从Product实体创建ProductDto的工厂构造函数
    /// </summary>
    public static ProductDto FromProduct(Database.Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            UploaderUserId = product.UploaderUserId,
            UploaderName = product.User.Name,
            CreateTime = product.CreateTime
        };
    }
    
    /// <summary>
    /// 从Product实体列表创建ProductDto列表
    /// </summary>
    public static List<ProductDto> FromProductList(List<Database.Product> products)
    {
        return products.Select(p => FromProduct(p)).ToList();
    }
}

public class ProductCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid CoverImageId { get; set; }
    public DateTime CreateTime { get; set; }
}

/// <summary>
/// 分类搜索结果DTO
/// </summary>
public class CategorySearchResultDto
{
    public List<string> Categories { get; set; }
    
    public CategorySearchResultDto(List<string> categories)
    {
        Categories = categories;
    }
}

// admin
public class UserDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Guid Avatar { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsAdmin { get; set; }
    public string? BlockReason { get; set; }
    public DateTime? BlockedAt { get; set; }
    public DateTime CreateTime { get; set; }
    
    /// <summary>
    /// 从User实体创建UserDto的工厂构造函数
    /// </summary>
    public static UserDto FromUser(Database.User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            Avatar = user.Avatar,
            IsBlocked = user.IsBlocked,
            IsAdmin = user.IsAdmin,
            BlockReason = user.BlockReason,
            BlockedAt = user.BlockedAt,
            CreateTime = user.CreateTime
        };
    }
    
    /// <summary>
    /// 从User实体列表创建UserDto列表
    /// </summary>
    public static List<UserDto> FromUserList(List<Database.User> users)
    {
        return users.Select(u => FromUser(u)).ToList();
    }
}

public class UserDetailDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Guid Avatar { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsAdmin { get; set; }
    public string? BlockReason { get; set; }
    public DateTime? BlockedAt { get; set; }
    public DateTime CreateTime { get; set; }
    
    /// <summary>
    /// 从User实体创建UserDetailDto的工厂构造函数
    /// </summary>
    public static UserDetailDto FromUser(Database.User user)
    {
        return new UserDetailDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            Avatar = user.Avatar,
            IsBlocked = user.IsBlocked,
            IsAdmin = user.IsAdmin,
            BlockReason = user.BlockReason,
            BlockedAt = user.BlockedAt,
            CreateTime = user.CreateTime
        };
    }
}

public class SystemStatisticsDto
{
    public int TotalUsers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalTransactions { get; set; }
    public int RecentTransactions { get; set; }
    public int BannedUsers { get; set; }
}
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。

// finished by hr