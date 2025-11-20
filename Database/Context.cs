using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Password).IsRequired();
        });
        modelBuilder.Entity<Session>(entity =>
        {
            // 设置Token作为主键
            entity.HasKey(e => e.Token);
            
            // 设置Token的最大长度
            entity.Property(e => e.Token).HasMaxLength(255).IsRequired();
            
            // 配置与User的关系
            entity.HasOne(e => e.User)
                  .WithMany() // 一个用户可以有多个会话
                  .HasForeignKey(e => e.UserId)
                  .IsRequired();
            
            // 为Token添加索引，提高查询性能
            entity.HasIndex(e => e.Token).IsUnique();
            
            // 为UserId添加索引，便于通过用户查找会话
            entity.HasIndex(e => e.UserId);
            
            // 为过期时间添加索引，便于清理过期会话
            entity.HasIndex(e => e.ExpiresAt);
        });
    }
}