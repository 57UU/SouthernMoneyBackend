using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Database;

public static class JwtUtils
{
    public const string ADMIN_ROLE = "Admin";
    public const string USER_ROLE = "User";
        
    private static readonly string SecretKey = GenerateSecretKey();
    private static readonly string Issuer = "SouthernMoneyBackend";        
    private static readonly string Audience = "SouthernMoneyFrontend";        
    private static readonly int TokenExpiryHours = 24*7;
    private static string GenerateSecretKey()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 生成JWT令牌
    /// </summary>
    public static string GenerateToken(long userId, bool isAdmin = false)
    {
        Claim[] claims = isAdmin ? 
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, ADMIN_ROLE),
                new Claim(ClaimTypes.Role, USER_ROLE)
            ] : 
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, USER_ROLE)
            ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(TokenExpiryHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 验证JWT令牌并返回ClaimsPrincipal
    /// </summary>
    public static ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = key
            };

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 从ClaimsPrincipal中获取用户ID
    /// </summary>
    public static long? GetUserId(ClaimsPrincipal principal)
    {
        var userIdString = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdString, out var userId))
        {
            return userId;
        }
        return null;
    }



    /// <summary>
    /// 检查用户是否为管理员
    /// </summary>
    public static bool IsAdmin(ClaimsPrincipal principal)
    {
        return principal?.IsInRole(ADMIN_ROLE) ?? false;
    }
}