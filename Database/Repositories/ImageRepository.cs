using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

/// <summary>
/// 图片数据访问层
/// </summary>
public class ImageRepository
{
    private readonly AppDbContext _context;
    
    public ImageRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// 添加新图片
    /// </summary>
    public async Task<Image> AddImageAsync(Image image)
    {
        _context.Images.Add(image);
        await _context.SaveChangesAsync();
        return image;
    }
    
    /// <summary>
    /// 根据ID获取图片
    /// </summary>
    public async Task<Image?> GetImageByIdAsync(Guid id)
    {
        return await _context.Images
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id);
    }
    
    /// <summary>
    /// 更新图片
    /// </summary>
    public async Task<Image> UpdateImageAsync(Image image)
    {
        _context.Images.Update(image);
        await _context.SaveChangesAsync();
        return image;
    }
    
    /// <summary>
    /// 删除图片
    /// </summary>
    public async Task<bool> DeleteImageAsync(Guid id)
    {
        var image = await GetImageByIdAsync(id);
        if (image == null)
        {
            return false;
        }
        
        _context.Images.Remove(image);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// 获取用户的所有图片
    /// </summary>
    public async Task<List<Image>> GetImagesByUserIdAsync(long userId, int page = 1, int pageSize = 10)
    {
        return await _context.Images
            .Include(i => i.User)
            .Where(i => i.UploaderUserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// 获取所有图片（分页）
    /// </summary>
    public async Task<List<Image>> GetAllImagesAsync(int page = 1, int pageSize = 10)
    {
        return await _context.Images
            .Include(i => i.User)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// 根据图片类型获取图片
    /// </summary>
    public async Task<List<Image>> GetImagesByTypeAsync(string imageType, int page = 1, int pageSize = 10)
    {
        return await _context.Images
            .Include(i => i.User)
            .Where(i => i.ImageType == imageType)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// 根据关键词搜索图片描述
    /// </summary>
    public async Task<List<Image>> SearchImagesByDescriptionAsync(string keyword, int page = 1, int pageSize = 10)
    {
        return await _context.Images
            .Include(i => i.User)
            .Where(i => i.Description != null && i.Description.Contains(keyword))
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// 检查图片是否属于指定用户
    /// </summary>
    public async Task<bool> IsImageOwnedByUserAsync(Guid imageId, long userId)
    {
        return await _context.Images
            .AnyAsync(i => i.Id == imageId && i.UploaderUserId == userId);
    }
    
    /// <summary>
    /// 获取图片总数
    /// </summary>
    public async Task<int> GetImageCountAsync()
    {
        return await _context.Images.CountAsync();
    }
    
    /// <summary>
    /// 获取用户的图片总数
    /// </summary>
    public async Task<int> GetUserImageCountAsync(long userId)
    {
        return await _context.Images
            .Where(i => i.UploaderUserId == userId)
            .CountAsync();
    }
}