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
    private readonly Database.Repositories.UserRepository _userRepository;
    
    public UserService(Database.Repositories.UserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    /// <summary>
    /// register a new user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task RegisterUser(Database.User user)
    {
        // 检查用户名是否已存在
        var existingUser = await _userRepository.GetUserByNameAsync(user.Name);
        if (existingUser != null)
        {
            throw new Exception("Username already exists");
        }
        
        //hash passwd
        user.Password = Utils.HashPassword(user.Password);
        await _userRepository.AddUserAsync(user);
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
        var user = await _userRepository.GetUserByNameAsync(name) 
            ?? throw new Exception("User not found");
            
        //verify password
        if (!Utils.VerifyPassword(password, user.Password))
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
        return await _userRepository.GetUserByIdAsync(userId.Value);
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
        var user = await _userRepository.GetUserByIdAsync(userId.Value);
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
    /// <param name="userId"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task UpdateUser(long userId, Database.User user)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if (existingUser == null)
        {
            throw new Exception("User not found");
        }
        
        // 如果用户名有变化，检查新用户名是否已存在
        if (existingUser.Name != user.Name)
        {
            var userWithSameName = await _userRepository.GetUserByNameAsync(user.Name);
            if (userWithSameName != null && userWithSameName.Id != userId)
            {
                throw new Exception("Username already exists");
            }
        }
        
        // 更新用户信息
        existingUser.Name = user.Name;
        
        // 如果提供了新密码，则更新密码
        if (!string.IsNullOrEmpty(user.Password))
        {
            existingUser.Password = Utils.HashPassword(user.Password);
        }
        
        await _userRepository.UpdateUserAsync(existingUser);
    }
    
    /// <summary>
    /// 获取所有用户
    /// </summary>
    public async Task<List<Database.User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }
    
    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    public async Task<Database.User?> GetUserByIdAsync(long id)
    {
        return await _userRepository.GetUserByIdAsync(id);
    }
    
    /// <summary>
    /// 删除用户
    /// </summary>
    public async Task<bool> DeleteUserAsync(long id)
    {
        return await _userRepository.DeleteUserAsync(id);
    }
}