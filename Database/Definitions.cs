namespace Database;

public class User
{
    public long Id { get; set; }
    public string Name { get; set; }
    //this should be hashed
    public string Password { get; set; }
    public static User CreateUser(string name, string password)
    {
        return new User
        {
            Id = new Random().NextInt64(),
            Name = name,
            Password = password
        };
    }
}
public class Session
{
    // 使用Token作为主键
    public string Token { get; set; }
    
    // 用户ID外键
    public long UserId { get; set; }
    
    // 用户导航属性
    public User User { get; set; }
    
    // 创建时间
    public DateTime CreatedAt { get; set; }
    
    // 过期时间
    public DateTime ExpiresAt { get; set; }
    
}