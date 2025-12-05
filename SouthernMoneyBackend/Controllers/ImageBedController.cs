using Microsoft.AspNetCore.Mvc;
using Database;
using Service;
using SouthernMoneyBackend.Utils;
using Microsoft.AspNetCore.Authorization;
using SouthernMoneyBackend.Middleware;

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
    [AuthorizeUser]
    public async Task<ApiResponse<object>> UploadImageAsync([FromForm]UploadImageRequest request)
    {
        if(request.File==null)
        {
            return ApiResponse.Fail("File is null", "FILE_NULL");
        }
        if(request.File.Length>MaxFileSize)
        {
            return ApiResponse.Fail("File size exceeds 2MB", "FILE_TOO_LARGE");
        }
        using var stream = request.File.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var userId = HttpContext.GetUserId();
        var imageId = await imageBedService.UploadImageAsync(memoryStream.ToArray(),userId,request.ImageType,request.Description);
        return ApiResponse.Ok(new { ImageId = imageId });
    }
    /// <summary>
    ///     获取图片,不遵循认证中间件、返回格式。直接返回图片
    /// </summary>
    /// <param name="imageId"></param>
    /// <returns></returns>
    [HttpGet("get")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImageAsync([FromQuery(Name = "id")] Guid imageId)
    {
        var image = await imageBedService.GetImageAsync(imageId);
        if(image==null)
        {
            return NotFound();
        }
        return File(image.Data, "image/jpeg");
    }
}
