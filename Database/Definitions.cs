using System.Text.Json.Serialization;

namespace Database;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public class User
{
    public long Id { get; set; }

    public string Name { get; set; }
                              
    [JsonIgnore]
    public string Password { get; set; }//this should be hashed
    public static User CreateUser(string name, string password)
    {
        return new User
        {
            Id = new Random().NextInt64(),
            Name = name,
            Password = password
        };
    }
    public bool IsAdmin { get; set; } = false;
}



public class Image{
    public Guid Id { get; set; }
    // 用户ID外键
    public long UploaderUserId { get; set; }
    // 用户导航属性
    [JsonIgnore]
    public User User { get; set; }
    public DateTime CreatedAt { get; set; }= DateTime.Now;
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
    public DateTime CreatedAt { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public ICollection<PostImage> PostImages { get; set; }
    public ICollection<PostTags> PostTags { get; set; }
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


#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。