using Microsoft.EntityFrameworkCore;

namespace Service;

public class ImageBedService
{
    private readonly Database.Repositories.ImageRepository imageRepository;
    
    public ImageBedService(Database.Repositories.ImageRepository imageRepository)
    {
        this.imageRepository = imageRepository;
    }
    
    public async Task<Guid> UploadImageAsync(byte[] imageData, long userId, string imageType, string? description = null)
    {
        var image = new Database.Image
        {
            Id = Guid.NewGuid(),
            UploaderUserId = userId,
            Data = imageData,
            ImageType = imageType,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
        var imageId = await imageRepository.AddImageAsync(image);
        
        return imageId.Id;
    }
    
    public async Task<Database.Image?> GetImageAsync(Guid imageId)
    {
        return await imageRepository.GetImageByIdAsync(imageId);
    }
    
    public async Task<ICollection<Database.Image>> GetImagesByUser(long userId)
    {
        return await imageRepository.GetImagesByUserIdAsync(userId);
    }
    
    public async Task DeleteImageAsync(Guid imageId, long userId)
    {
        var image = await imageRepository.GetImageByIdAsync(imageId);
        if (image == null)
        {
            throw new ArgumentException("Image not found");
        }
        
        // 检查图片是否属于当前用户
        if (!await imageRepository.IsImageOwnedByUserAsync(imageId, userId))
        {
            throw new UnauthorizedAccessException("You can only delete your own images");
        }
        
        await imageRepository.DeleteImageAsync(imageId);
    }
    
    public async Task<ICollection<Database.Image>> GetAllImagesAsync(int page = 1, int pageSize = 20)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // 限制最大页面大小
        
        return await imageRepository.GetAllImagesAsync(page, pageSize);
    }
    
    public async Task<ICollection<Database.Image>> GetImagesByTypeAsync(string imageType, int page = 1, int pageSize = 20)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // 限制最大页面大小
        
        return await imageRepository.GetImagesByTypeAsync(imageType, page, pageSize);
    }
    
    public async Task<ICollection<Database.Image>> SearchImagesAsync(string searchTerm, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<Database.Image>();
        }
        
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // 限制最大页面大小
        
        return await imageRepository.SearchImagesByDescriptionAsync(searchTerm, page, pageSize);
    }
    
    public async Task<int> GetImageCountAsync()
    {
        return await imageRepository.GetImageCountAsync();
    }
    
    public async Task<int> GetUserImageCountAsync(long userId)
    {
        return await imageRepository.GetUserImageCountAsync(userId);
    }
    
    public async Task<bool> UpdateImageDescriptionAsync(Guid imageId, long userId, string description)
    {
        var image = await imageRepository.GetImageByIdAsync(imageId);
        if (image == null)
        {
            return false;
        }
        
        // 检查图片是否属于当前用户
        if (!await imageRepository.IsImageOwnedByUserAsync(imageId, userId))
        {
            throw new UnauthorizedAccessException("You can only edit your own images");
        }
        
        image.Description = description;
        image.CreatedAt = DateTime.UtcNow;
        
        await imageRepository.UpdateImageAsync(image);
        return true;
    }
}