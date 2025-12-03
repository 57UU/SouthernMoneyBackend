//following is written by hr
using Microsoft.AspNetCore.Mvc;
using SouthernMoneyBackend.Utils;
using SouthernMoneyBackend.Middleware;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/user")]
[AuthorizeUser]
public class UserController : ControllerBase
{
    public UserController()
    {
    }

    // GET /user/profile
    [HttpGet("profile")]
    public async Task<ApiResponse<UserProfileDto>> GetProfile()
    {
        throw new NotImplementedException();
    }

    // POST /user/update
    [HttpPost("update")]
    public async Task<ApiResponse<object>> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        throw new NotImplementedException();
    }

    // POST /user/changePassword
    [HttpPost("changePassword")]
    public async Task<ApiResponse<object>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        throw new NotImplementedException();
    }

    // POST /user/uploadAvatar
    [HttpPost("uploadAvatar")]
    public async Task<ApiResponse<object>> UploadAvatar([FromBody] UploadAvatarRequest file)
    {
        throw new NotImplementedException();
    }

    // POST /user/topup
    [HttpPost("topup")]
    public async Task<ApiResponse<object>> Topup([FromBody] TopUpRequest request)
    {
        throw new NotImplementedException();
    }
    
    // POST /user/openAccount
    [HttpPost("openAccount")]
    public async Task<ApiResponse<object>> OpenAccount()
    {
        throw new NotImplementedException();
    }
}
