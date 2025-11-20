

namespace Database;

public static class Utils
{
    /// <summary>
    /// 使用BCrypt算法哈希密码
    /// </summary>
    /// <param name="password">原始密码</param>
    /// <returns>哈希后的密码字符串</returns>
    public static string HashPassword(string password)
    {
        // 生成随机盐值并哈希密码
        // 使用12轮工作因子，平衡安全性和性能
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        return BCrypt.Net.BCrypt.HashPassword(password, salt);
    }
    
    /// <summary>
    /// 验证密码是否匹配哈希值
    /// </summary>
    /// <param name="password">原始密码</param>
    /// <param name="hashedPassword">哈希后的密码</param>
    /// <returns>密码是否匹配</returns>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}