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
    public async Task<long> RegisterUser(Database.User user, bool existIsOk = false)
    {
        // 检查用户名是否已存在
        var existingUser = await _userRepository.GetUserByNameAsync(user.Name);
        if (existingUser != null)
        {
            if (existIsOk)
            {
                return existingUser.Id;
            }
            else
            {
                throw new Exception("Username already exists");
            }
        }
        else
        {
            //hash passwd
            user.Password = Utils.HashPassword(user.Password);
            await _userRepository.AddUserAsync(user);
            return user.Id;
        }
    }
    /// <summary>
    /// verify password
    /// </summary>
    /// <param name="password"></param>
    /// <param name="hashedPassword"></param>
    /// <returns></returns>
    private static bool VerifyPassword(string password, string hashedPassword)
    {
        return Utils.VerifyPassword(password, hashedPassword);
    }
    public async Task UpdatePassword(long userId, string newPassword, string currentPassword)
    {

        var user = await _userRepository.GetUserByIdAsync(userId) ?? throw new Exception("User not found");
        // verify current password
        if (!VerifyPassword(currentPassword, user.Password))
        {
            throw new Exception("Current password is incorrect");
        }
        user.Password = Utils.HashPassword(newPassword);
        await _userRepository.UpdateUserAsync(user);
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
    /// update user info，but not password
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

        // 使用细粒度更新方式，只更新变化的字段
        var propertiesToUpdate = new Dictionary<string, object>();

        // 检查并添加需要更新的字段
        if (existingUser.Name != user.Name)
        {
            propertiesToUpdate.Add("Name", user.Name);
        }

        if (existingUser.Email != user.Email)
        {
            propertiesToUpdate.Add("Email", user.Email);
        }

        if (existingUser.Avatar != user.Avatar)
        {
            propertiesToUpdate.Add("Avatar", user.Avatar);
        }

        // 如果有需要更新的字段，执行更新
        if (propertiesToUpdate.Count > 0)
        {
            await _userRepository.UpdateUserPropertiesAsync(userId, propertiesToUpdate);
        }
    }
    public async Task UpdateUserAvater(long userId, Guid? avatarId)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if (existingUser == null)
        {
            throw new Exception("User not found");
        }

        await _userRepository.UpdateUserAvatarAsync(userId, avatarId);
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

    /// <summary>
    /// 更新用户邮箱
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="email">新邮箱</param>
    /// <returns></returns>
    public async Task UpdateUserEmail(long userId, string? email)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if (existingUser == null)
        {
            throw new Exception("User not found");
        }

        await _userRepository.UpdateUserEmailAsync(userId, email);
    }

    /// <summary>
    /// 更新用户头像
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="avatarId">新头像ID</param>
    /// <returns></returns>
    public async Task UpdateUserAvatar(long userId, Guid? avatarId)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if (existingUser == null)
        {
            throw new Exception("User not found");
        }

        await _userRepository.UpdateUserAvatarAsync(userId, avatarId);
    }

    /// <summary>
    /// 更新用户状态（封禁/解封）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="isBlocked">是否封禁</param>
    /// <param name="blockReason">封禁原因（封禁时必填）</param>
    /// <returns></returns>
    public async Task UpdateUserBlockStatus(long userId, bool isBlocked, string? blockReason = null)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if (existingUser == null)
        {
            throw new Exception("User not found");
        }

        var propertiesToUpdate = new Dictionary<string, object>
        {
            { "IsBlocked", isBlocked }
        };

        if (isBlocked)
        {
            propertiesToUpdate.Add("BlockedAt", DateTime.UtcNow);
            if (!string.IsNullOrEmpty(blockReason))
            {
                propertiesToUpdate.Add("BlockReason", blockReason);
            }
        }
        else
        {
            propertiesToUpdate.Add("BlockedAt", null);
            propertiesToUpdate.Add("BlockReason", null);
        }

        await _userRepository.UpdateUserPropertiesAsync(userId, propertiesToUpdate);
    }

    /// <summary>
    /// 更新用户管理员状态
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="isAdmin">是否为管理员</param>
    /// <returns></returns>
    public async Task UpdateUserAdminStatus(long userId, bool isAdmin)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if (existingUser == null)
        {
            throw new Exception("User not found");
        }

        await _userRepository.UpdateUserPropertyAsync(userId, "IsAdmin", isAdmin);
    }

    /// <summary>
    /// 更新用户账户状态
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="hasAccount">是否已开户</param>
    /// <returns></returns>
    public async Task UpdateUserAccountStatus(long userId, bool hasAccount)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if (existingUser == null)
        {
            throw new Exception("User not found");
        }

        var propertiesToUpdate = new Dictionary<string, object>
        {
            { "HasAccount", hasAccount }
        };

        if (hasAccount && !existingUser.AccountOpenedAt.HasValue)
        {
            propertiesToUpdate.Add("AccountOpenedAt", DateTime.UtcNow);
        }

        await _userRepository.UpdateUserPropertiesAsync(userId, propertiesToUpdate);
    }

    /// <summary>
    /// 更新用户余额
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="balance">新余额</param>
    /// <returns></returns>
    public async Task UpdateUserBalance(long userId, decimal balance)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if (existingUser == null)
        {
            throw new Exception("User not found");
        }

        await _userRepository.UpdateUserPropertyAsync(userId, "Balance", balance);
    }
}