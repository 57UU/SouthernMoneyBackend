using Microsoft.AspNetCore.Mvc;
using Database;
using Service;
using SouthernMoneyBackend.Utils;
using Microsoft.AspNetCore.Authorization;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/images")]
public class ImageBedController : ControllerBase
{
    private readonly ImageBedService imageBedService;
    public ImageBedController(ImageBedService imageBedService)
    {
        this.imageBedService = imageBedService;
    }
    const int MaxFileSize = 1024 * 1024 * 2; // 2MB
    [HttpPost("upload")]
    public async Task<ApiResponse<object>> UploadImageAsync([FromForm]IFormFile file,string imageType,string? description=null)
    {
        if(file==null)
        {
            return ApiResponse.Fail("File is null", "FILE_NULL");
        }
        if(file.Length>MaxFileSize)
        {
            return ApiResponse.Fail("File size exceeds 2MB", "FILE_TOO_LARGE");
        }
        using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var userId = HttpContext.GetUserId();
        var imageId = await imageBedService.UploadImageAsync(memoryStream.ToArray(),userId,imageType,description);
        return ApiResponse.Ok(new { ImageId = imageId });
    }
    [HttpGet("get")]
    [AllowAnonymous]
    public async Task<ApiResponse<object>> GetImageAsync([FromQuery(Name = "id")] Guid imageId)
    {
        var image = await imageBedService.GetImageAsync(imageId);
        if(image==null)
        {
            return ApiResponse.Fail("Image not found", "IMAGE_NOT_FOUND");
        }
        return ApiResponse.Ok(image);
    }
}
