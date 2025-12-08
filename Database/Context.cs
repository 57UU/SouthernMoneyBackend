using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostImage> PostImages { get; set; }
    public DbSet<PostTags> PostTags { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<PostBlock> PostBlocks { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<TransactionRecord> TransactionRecords { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<UserFavoriteCategory> UserFavoriteCategories { get; set; }
    public DbSet<UserAsset> UserAssets { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<PostFavorite> PostFavorites { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 配置User实体
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Password).IsRequired();
            entity.Property(e => e.IsAdmin).HasDefaultValue(false);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Balance).HasDefaultValue(0);
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
            entity.Property(e => e.CreateTime).IsRequired();
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
        
        // 配置PostLike实体
        modelBuilder.Entity<PostLike>(entity =>
        {
            entity.HasKey(pl => new { pl.PostId, pl.UserId });
            
            entity.HasOne(pl => pl.Post)
                  .WithMany(p => p.PostLikes)
                  .HasForeignKey(pl => pl.PostId)
                  .IsRequired();
            
            entity.HasOne(pl => pl.User)
                  .WithMany(u => u.PostLikes)
                  .HasForeignKey(pl => pl.UserId)
                  .IsRequired();
        });
        
        // 配置Product实体
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Products)
                  .HasForeignKey(e => e.UploaderUserId)
                  .IsRequired();
            
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .IsRequired();
            
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Price).IsRequired();
            entity.Property(e => e.Description).IsRequired();
        });
        
        // 配置UserFavoriteCategory实体
        modelBuilder.Entity<UserFavoriteCategory>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.CategoryId });
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.FavoriteCategories)
                  .HasForeignKey(e => e.UserId)
                  .IsRequired();
            
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.FavoriteUsers)
                  .HasForeignKey(e => e.CategoryId)
                  .IsRequired();
        });
        
        // 配置TransactionRecord实体
        modelBuilder.Entity<TransactionRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .IsRequired();
            
            entity.HasOne(e => e.Buyer)
                  .WithMany(u => u.PurchasedProducts)
                  .HasForeignKey(e => e.BuyerUserId)
                  .IsRequired();
            
            entity.Property(e => e.Price).IsRequired();
            entity.Property(e => e.TotalPrice).IsRequired();
        });
        
        // 配置ProductCategory实体
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CoverImageId).IsRequired();
        });
        
        // 配置UserAsset实体
        modelBuilder.Entity<UserAsset>(entity =>
        {
            entity.HasKey(e => e.UserId);
            
            entity.HasOne(e => e.User)
                  .WithOne()
                  .HasForeignKey<UserAsset>(e => e.UserId)
                  .IsRequired();
        });
        
        // 配置Notification实体
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.User)
                  .WithMany() // 一个用户可以有多个通知
                  .HasForeignKey(e => e.UserId)
                  .IsRequired();
            
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.SubjectUserId);
        });
        
        // 配置PostBlock实体
        modelBuilder.Entity<PostBlock>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // 配置与Post的关系
            entity.HasOne(e => e.Post)
                  .WithMany(p => p.PostBlocks) // 一个帖子可以有多个操作记录
                  .HasForeignKey(e => e.PostId)
                  .IsRequired();
            
            // 配置与AdminUser的关系
            entity.HasOne(e => e.AdminUser)
                  .WithMany() // 一个管理员可以操作多个帖子
                  .HasForeignKey(e => e.AdminUserId)
                  .IsRequired();
            
            entity.Property(e => e.Reason).IsRequired();
            entity.Property(e => e.ActionTime).IsRequired();
        });
        
        // 配置PostFavorite实体
        modelBuilder.Entity<PostFavorite>(entity =>
        {
            entity.HasKey(pf => new { pf.PostId, pf.UserId });
            
            entity.HasOne(pf => pf.Post)
                  .WithMany(p => p.PostFavorites)
                  .HasForeignKey(pf => pf.PostId)
                  .IsRequired();
            
            entity.HasOne(pf => pf.User)
                  .WithMany(u => u.PostFavorites)
                  .HasForeignKey(pf => pf.UserId)
                  .IsRequired();
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
    }

}

