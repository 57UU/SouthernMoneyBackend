using Microsoft.EntityFrameworkCore;

namespace Service;

public class ImageBedService
{
    private readonly Database.AppDbContext dbContext;
    public ImageBedService(Database.AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    public async Task<Guid> UploadImageAsync(byte[] imageData,long userId,string imageType,string? description=null)
    {
        var image = new Database.Image
        {
            Id = Guid.NewGuid(),
            UploaderUserId = userId,
            Data = imageData,
            ImageType = imageType,
            Description = description
        };
        dbContext.Images.Add(image);
        await dbContext.SaveChangesAsync();
        return image.Id;
    }
    public async Task<Database.Image?> GetImageAsync(Guid imageId)
    {
        return await dbContext.Images.FindAsync(imageId);
    }
    public async Task<ICollection<Database.Image>> GetImagesByUser(long userId)
    {
        return await dbContext.Images.Where(x=>x.UploaderUserId==userId).ToArrayAsync();
    }
    public async Task DeleteImageAsync(Guid imageId)
    {
        var image = await dbContext.Images.FindAsync(imageId);
        if(image==null)
        {
            throw new ArgumentException("Image not found");
        }
        dbContext.Images.Remove(image);
        await dbContext.SaveChangesAsync();
    }
}