using Microsoft.AspNetCore.Mvc;
using Database;
using SouthernMoneyBackend.Utils;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/login")] // api in this scope do not require verification
public class LoginController : ControllerBase
{
    private readonly UserService userService;
    public LoginController(UserService userService)
    {
        this.userService = userService;
    }
    [HttpPost("register", Name = "RegisterUser")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = Database.User.CreateUser(request.Name, request.Password);
        try
        {
            await userService.RegisterUser(user);
            return Ok(ApiResponse.Ok());
        }
        catch (Exception e)
        {
            return BadRequest(ApiResponse.Fail(e.Message, "REGISTER_FAILED"));
        }
    }
    [HttpPost("loginByPassword", Name = "LoginByPassword")]
    public async Task<IActionResult> LoginByPassword([FromBody] LoginByPasswordRequest request)
    {
        try
        {
            var token = await userService.LoginByPassword(request.Name, request.Password);
            return Ok(ApiResponse.Ok(new { token = token }));
        }
        catch (Exception e)
        {
            return BadRequest(ApiResponse.Fail(e.Message, "LOGIN_FAILED"));
        }
    }
    
    [HttpPost("refreshToken", Name = "RefreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] LoginByTokenRequest request)
    {
        try
        {
            var newToken = await userService.RefreshToken(request.Token);
            if (newToken != null)
            {
                return Ok(ApiResponse.Ok(new { token = newToken }));
            }
            else
            {
                return BadRequest(ApiResponse.Fail("Failed to refresh token", "REFRESH_TOKEN_FAILED"));
            }
        }
        catch (Exception e)
        {
            return BadRequest(ApiResponse.Fail(e.Message, "REFRESH_TOKEN_ERROR"));
        }
    }

}
