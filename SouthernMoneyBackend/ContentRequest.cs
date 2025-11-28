using Database;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public class RegisterRequest
{
    public string Name { get; set; }
    public string Password { get; set; }
}
public class LoginByPasswordRequest
{
    public string Name { get; set; }
    public string Password { get; set; }
}
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
}
public class PostRequest{
    public string Title { get; set; }
    public string Content { get; set; }
    public ICollection<string> Images { get; set; }
    public ICollection<string> Tags { get; set; }
}

public class PostReportRequest{
    public Guid PostId { get; set; }
    public string Reason { get; set; }
}
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
