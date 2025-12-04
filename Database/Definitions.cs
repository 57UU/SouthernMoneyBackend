using System.Text.Json.Serialization;

namespace Database;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public class User
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string? Email { get; set; }
    
    public Guid? Avatar { get; set; }
                              
    [JsonIgnore]
    public string Password { get; set; }//this should be hashed
    public static User CreateUser(string name, string password)
    {
        return new User
        {
            Id = new Random().NextInt64(),
            Name = name,
            Password = password,
            IsAdmin = false,
            HasAccount = false,
            IsBlocked = false,
            BlockReason = null,
            BlockedAt = null,
            CreateTime = DateTime.UtcNow,
            IsDeleted = false
        };
    }
    public bool IsAdmin { get; set; } = false;

    //新增：是否开户
    public bool HasAccount { get; set; } = false;

    //开户时间
    public DateTime? AccountOpenedAt { get; set; }  

    //是否封禁
    public bool IsBlocked { get; set; } = false;

    //封禁原因（可选）
    public string? BlockReason { get; set; }

    //封禁时间
    public DateTime? BlockedAt { get; set; }

    //创建时间
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    //是否删除
    public bool IsDeleted { get; set; } = false;

    //余额
    public decimal Balance { get; set; } = 0;

    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<TransactionRecord> PurchasedProducts { get; set; } = new List<TransactionRecord>();
    public ICollection<UserFavoriteCategory> FavoriteCategories { get; set; } = new List<UserFavoriteCategory>();
    public UserAsset? Asset { get; set; }
}



public class Image{
    public Guid Id { get; set; }
    // 用户ID外键
    public long UploaderUserId { get; set; }
    // 用户导航属性
    [JsonIgnore]
    public User User { get; set; }
    public DateTime CreateTime { get; set; }= DateTime.Now;
    public string? Description { get; set; }
    public string ImageType { get; set; }
    public byte[] Data { get; set; }
}

public class Post{
    public Guid Id { get; set; }
    // 用户ID外键
    public long UploaderUserId { get; set; }
    // 用户导航属性
    [JsonIgnore]
    public User User { get; set; }
    public DateTime CreateTime { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int ReportCount { get; set; } = 0;
    public int ViewCount { get; set; } = 0;
    public int LikeCount { get; set; } = 0;
    public bool IsBlocked { get; set; } = false;
    public ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();
    public ICollection<PostTags> PostTags { get; set; } = new List<PostTags>();
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
}

public class PostImage
{
    public Guid PostId { get; set; }

    public Post Post { get; set; }
    
    public Guid ImageId { get; set; }

    public Image Image { get; set; }
}

public class PostTags{
    public Guid PostId { get; set; }
    public Post Post { get; set; }
    public string Tag { get; set; }
}

public class PostLike
{
    public Guid PostId { get; set; }
    public Post Post { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
}

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
    public ProductCategory Category { get; set; }
    public long UploaderUserId { get; set; }
    public User User { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}

public class UserFavoriteCategory
{
    public long UserId { get; set; }
    public User User { get; set; }
    
    public Guid CategoryId { get; set; }
    public ProductCategory Category { get; set; }
    
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
}

public class TransactionRecord
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public long BuyerUserId { get; set; }
    public User Buyer { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime PurchaseTime { get; set; } = DateTime.UtcNow;
}

public class ProductCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid CoverImageId { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<UserFavoriteCategory> FavoriteUsers { get; set; } = new List<UserFavoriteCategory>();
}

public class UserAsset
{
    public long UserId { get; set; }
    public User User { get; set; }
    public decimal Total { get; set; } = 0;
    public decimal TodayEarn { get; set; } = 0;
    public decimal AccumulatedEarn { get; set; } = 0;
    public decimal EarnRate { get; set; } = 0;
    public decimal Balance { get; set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
