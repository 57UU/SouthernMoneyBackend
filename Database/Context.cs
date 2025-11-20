using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostImage> PostImages { get; set; }

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
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id);
            // 配置与User的关系
            entity.HasOne(e => e.User)
                  .WithMany() // 一个用户可以上传多个图片
                  .HasForeignKey(e => e.UploaderUserId)
                  .IsRequired();
            entity.Property(e => e.Description);
            entity.Property(e => e.ImageType).IsRequired();
            entity.Property(e => e.Data).IsRequired();
        });
        
        // 配置Post实体
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            // 配置与User的关系
            entity.HasOne(e => e.User)
                  .WithMany() // 一个用户可以发布多个帖子
                  .HasForeignKey(e => e.UploaderUserId)
                  .IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
        
        // 配置PostImage实体作为多对多关联表
        modelBuilder.Entity<PostImage>(entity =>
        {
            // 设置复合主键
            entity.HasKey(pi => new { pi.PostId, pi.ImageId });
            
            // 配置与Post的关系
            entity.HasOne(pi => pi.Post)
                  .WithMany(pi => pi.PostImages)
                  .HasForeignKey(pi => pi.PostId)
                  .IsRequired();
            
            // 配置与Image的关系
            entity.HasOne(pi => pi.Image)
                  .WithMany()
                  .HasForeignKey(pi => pi.ImageId)
                  .IsRequired();
        });
        // 配置PostTags实体
        modelBuilder.Entity<PostTags>(entity =>
        {
            // 设置复合主键
            entity.HasKey(pt => new { pt.PostId, pt.Tag });
            
            // 配置与Post的关系
            entity.HasOne(pt => pt.Post)
                  .WithMany(pt => pt.PostTags)
                  .HasForeignKey(pt => pt.PostId)
                  .IsRequired();
            
            // 配置Tag属性
            entity.Property(pt => pt.Tag).IsRequired();
        });
    }

}