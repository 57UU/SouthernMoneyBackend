namespace SouthernMoneyBackend;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。


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
}


#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
