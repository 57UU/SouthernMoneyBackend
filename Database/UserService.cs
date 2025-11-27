using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database;

/// <summary>
/// manage user and session
/// </summary>
public class UserService
{
    private AppDbContext context;
    public UserService(AppDbContext context)
    {
        this.context = context;
    }
    /// <summary>
    /// register a new user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task RegisterUser(User user)
    {
        //hash passwd
        user.Password = Utils.HashPassword(user.Password);
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
    public async Task<string> LoginByPassword(string name, string password)
    {
        var user= await context.Users.FirstOrDefaultAsync(u => u.Name == name) 
            ?? throw new Exception("User not found");
        //verify password
        if (!Utils.VerifyPassword(password, user.Password))
        {
            throw new Exception("Password not match");
        }
        
        // 生成JWT令牌
        return JwtUtils.GenerateToken(user.Id, user.IsAdmin);
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
    public async Task<User?> GetUserByToken(string token)
    {
        var userId = GetUserIdByToken(token);
        if (!userId.HasValue)
        {
            return null;
        }
        return await context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
    }
    
    /// <summary>
    /// 刷新JWT令牌
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<string?> RefreshToken(string token)
    {
        var user = await GetUserByToken(token);
        if (user == null)
        {
            return null;
        }
        return JwtUtils.GenerateToken(user.Id, user.IsAdmin);
    }
    /// <summary>
    /// update user info
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task UpdateUser(long userId, User user)
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
