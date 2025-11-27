using Microsoft.AspNetCore.Mvc;
using Database;
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
    [HttpPost("/upload")]
    public async Task<IActionResult> UploadImageAsync([FromForm]IFormFile file,string imageType,string? description=null)
    {
        if(file==null)
        {
            return BadRequest(ApiResponse.Fail("File is null", "FILE_NULL"));
        }
        if(file.Length>MaxFileSize)
        {
            return BadRequest(ApiResponse.Fail("File size exceeds 2MB", "FILE_TOO_LARGE"));
        }
        using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var userId = HttpContext.GetUserId();
        var imageId = await imageBedService.UploadImageAsync(memoryStream.ToArray(),userId,imageType,description);
        return Ok(ApiResponse.Ok(new { ImageId = imageId }));
    }
    [HttpGet("/get")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImageAsync([FromQuery(Name = "id")] Guid imageId)
    {
        var image = await imageBedService.GetImageAsync(imageId);
        if(image==null)
        {
            return NotFound(ApiResponse.Fail("Image not found", "IMAGE_NOT_FOUND"));
        }
        return Ok(ApiResponse.Ok(image));
    }
}
