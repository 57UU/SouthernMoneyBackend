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
    /// login by password and return a token
    /// </summary>
    /// <param name="name"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<Session> LoginByPassword(string name, string password)
    {
        var user= await context.Users.FirstOrDefaultAsync(u => u.Name == name);
        if(user == null)
        {
            throw new Exception("User not found");
        }
        //verify password
        if(!Utils.VerifyPassword(password, user.Password))
        {
            throw new Exception("Password not match");
        }
        var session = new Session
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        context.Sessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }
    /// <summary>
    /// validate token and refresh expires time
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<bool> LoginByToken(string token)
    {
        var session = await context.Sessions.FirstOrDefaultAsync(s => s.Token == token && s.ExpiresAt > DateTime.UtcNow);
        if (session == null)
        {
            return false;
        }
        session.ExpiresAt = DateTime.UtcNow.AddDays(7);
        await context.SaveChangesAsync();
        return true;
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
