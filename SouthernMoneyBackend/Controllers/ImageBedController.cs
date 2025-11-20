using Microsoft.AspNetCore.Mvc;
using Database;

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
    [HttpPost]
    public async Task<IActionResult> UploadImageAsync([FromForm]IFormFile file,string imageType,string? description=null)
    {
        if(file==null)
        {
            return BadRequest("File is null");
        }
        if(file.Length>MaxFileSize)
        {
            return BadRequest("File size exceeds 2MB");
        }
        using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var userId = HttpContext.GetUserId();
        var imageId = await imageBedService.UploadImageAsync(memoryStream.ToArray(),userId,imageType,description);
        return Ok(new {ImageId=imageId});
    }
    [HttpGet]
    public async Task<IActionResult> GetImageAsync(Guid imageId)
    {
        var image = await imageBedService.GetImageAsync(imageId);
        if(image==null)
        {
            return NotFound("Image not found");
        }
        return Ok(image);
    }
}