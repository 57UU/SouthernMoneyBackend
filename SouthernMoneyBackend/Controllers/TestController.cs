using Microsoft.AspNetCore.Mvc;
using SouthernMoneyBackend.Utils;
using Microsoft.AspNetCore.Authorization;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/test")]
public class TestController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public ApiResponse<object> Test()
    {
        return ApiResponse.Ok(new { Message = "Server is running" });
    }
}