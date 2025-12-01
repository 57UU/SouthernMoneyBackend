// this is all by hr
namespace SouthernMoneyBackend;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。

//login
public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ReportCount { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsLiked { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<Guid> ImageIds { get; set; } = new();
    public PostUploaderDto? Uploader { get; set; }
}

public class PostUploaderDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public Guid Avatar { get; set; }
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
//user
public class UserAssetDto
{
    public decimal Total { get; set; }
    public decimal TodayEarn { get; set; }
    public decimal AccumulatedEarn { get; set; }
    public decimal EarnRate { get; set; }
    public decimal Balance { get; set; }
}

public class UserProfileDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Guid Avatar { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserAssetDto Asset { get; set; }
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
}

public class PurchaseRecordsPageDto
{
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public List<PurchaseRecordDto> Records { get; set; } = new();
}
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。

// finished by hr