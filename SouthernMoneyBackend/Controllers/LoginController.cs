using Microsoft.AspNetCore.Mvc;
using Database;
using Service;
using SouthernMoneyBackend.Utils;
using Microsoft.AspNetCore.Authorization;

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
    [AllowAnonymous]
    public async Task<ApiResponse> Register([FromBody] RegisterRequest request)
    {
        var user = Database.User.CreateUser(request.Name, request.Password);
        try
        {
            await userService.RegisterUser(user);
            return ApiResponse.Ok();
        }
        catch (Exception e)
        {
            return ApiResponse.Fail(e.Message, "REGISTER_FAILED");
        }
    }
    [HttpPost("loginByPassword", Name = "LoginByPassword")]
    [AllowAnonymous]
    public async Task<ApiResponse<TokenResponseDto>> LoginByPassword([FromBody] LoginByPasswordRequest request)
    {
        try
        {
            var (token, refreshToken,user) = await userService.LoginByPassword(request.Name, request.Password);
            return ApiResponse<TokenResponseDto>.Ok(new TokenResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Id = user.Id
            });
        }
        catch (Exception e)
        {
            return ApiResponse<TokenResponseDto>.Fail(e.Message, "LOGIN_FAILED");
        }
    }

    [HttpPost("refreshToken", Name = "RefreshToken")]
    [AllowAnonymous]
    public async Task<ApiResponse<object>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var tokenPair = await userService.RefreshToken(request.RefreshToken);
            if (tokenPair != null)
            {
                var (newToken, newRefreshToken) = tokenPair.Value;
                return ApiResponse.Ok(new TokenResponseDto { Token = newToken, RefreshToken = newRefreshToken });
            }
            else
            {
                return ApiResponse.Fail("Failed to refresh token", "REFRESH_TOKEN_FAILED");
            }
        }
        catch (Exception e)
        {
            return ApiResponse.Fail(e.Message, "REFRESH_TOKEN_ERROR");
        }
    }

}
