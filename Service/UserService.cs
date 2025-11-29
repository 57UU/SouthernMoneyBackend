using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service;

/// <summary>
/// manage user and session
/// </summary>
public class UserService
{
    private Database.AppDbContext context;
    public UserService(Database.AppDbContext context)
    {
        this.context = context;
    }
    /// <summary>
    /// register a new user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task RegisterUser(Database.User user)
    {
        //hash passwd
        user.Password = Database.Utils.HashPassword(user.Password);
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
    /// <summary>
    /// login by password and return a JWT token
    /// </summary>
    /// <param name="name"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<(string token, string refreshToken)> LoginByPassword(string name, string password)
    {
        var user= await context.Users.FirstOrDefaultAsync(u => u.Name == name) 
            ?? throw new Exception("User not found");
        //verify password
        if (!Database.Utils.VerifyPassword(password, user.Password))
        {
            throw new Exception("Password not match");
        }
        
        // 生成JWT令牌
        var token = JwtUtils.GenerateToken(user.Id, user.IsAdmin);
        // 生成Refresh Token
        var refreshToken = JwtUtils.GenerateRefreshToken(user.Id);
        return (token, refreshToken);
    }
    /// <summary>
    /// validate JWT token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public bool ValidateToken(string token)
    {
        var principal = JwtUtils.ValidateToken(token);
        return principal != null;
    }
    
    /// <summary>
    /// 通过JWT token获取用户ID
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public long? GetUserIdByToken(string token)
    {
        var principal = JwtUtils.ValidateToken(token);
        if (principal == null)
        {
            return null;
        }
        return JwtUtils.GetUserId(principal);
    }
    
    /// <summary>
    /// 通过JWT token获取用户信息
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<Database.User?> GetUserByToken(string token)
    {
        var userId = GetUserIdByToken(token);
        if (!userId.HasValue)
        {
            return null;
        }
        return await context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
    }
    
    /// <summary>
    /// 使用Refresh Token刷新JWT令牌
    /// </summary>
    /// <param name="refreshToken">Refresh Token</param>
    /// <returns>新的Access Token和Refresh Token对，如果无效则返回null</returns>
    public async Task<(string token, string refreshToken)?> RefreshToken(string refreshToken)
    {
        // 验证refresh token并获取用户ID
        var userId = JwtUtils.ValidateRefreshToken(refreshToken);
        if (!userId.HasValue)
        {
            return null;
        }
        
        // 获取用户信息
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (user == null)
        {
            return null;
        }
        
        // 生成新的access token和refresh token
        var newToken = JwtUtils.GenerateToken(user.Id, user.IsAdmin);
        var newRefreshToken = JwtUtils.GenerateRefreshToken(user.Id);
        
        return (newToken, newRefreshToken);
    }
    /// <summary>
    /// update user info
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task UpdateUser(long userId, Database.User user)
    {
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (existingUser == null)
        {
            throw new Exception("User not found");
        }
        existingUser.Name = user.Name;
        existingUser.Password = user.Password;
        context.Users.Update(existingUser);
        await context.SaveChangesAsync();
    }
}