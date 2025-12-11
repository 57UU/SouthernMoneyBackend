//following is written by hr
using Database;
using Microsoft.AspNetCore.Mvc;
using Service;
using SouthernMoneyBackend.Middleware;
using SouthernMoneyBackend.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/user")]
[AuthorizeUser]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly UserAssetService _userAssetService;
    private readonly ImageBedService _imageBedService;
    
    public UserController(
        UserService userService,
        UserAssetService userAssetService,
        ImageBedService imageBedService)
    {
        _userService = userService;
        _userAssetService = userAssetService;
        _imageBedService = imageBedService;
    }

    // GET /user/profile
    [HttpGet("profile")]
    public async Task<ApiResponse<UserProfileDto>> GetProfile()
    {
        try
        {
            
            var userId=HttpContext.GetUserId();
            
            // 获取用户信息
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserProfileDto>.Fail("User not found");
            }
            
            // 获取用户资产信息
            UserAsset? asset;
            UserAssetDto assetDto;
            try
            {
                asset = await _userAssetService.GetUserAssetByUserIdAsync(userId);
                if (asset != null)
                {
                    assetDto = UserAssetDto.FromUserAsset(asset);
                }
                else
                {
                    return ApiResponse<UserProfileDto>.Fail("尚未开户 请先开户");
                }
            }
            catch
            {
                // 如果获取资产信息失败，使用默认值
                assetDto = UserAssetDto.CreateDefault();
            }
            
            // 使用工厂构造函数构建返回的DTO
            var userProfileDto = UserProfileDto.FromUser(user, assetDto);
            
            return ApiResponse<UserProfileDto>.Ok(userProfileDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<UserProfileDto>.Fail(ex.Message);
        }
    }

    // POST /user/update
    [HttpPost("update")]
    public async Task<ApiResponse> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            
            var userId=HttpContext.GetUserId();
            
            // 获取当前用户信息
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.Fail("User not found");
            }
            
            // 更新用户信息
            user.Name = request.Name;
            user.Email = request.Email;
            
            await _userService.UpdateUser(userId, user);
            
            return ApiResponse.Ok();
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail(ex.Message);
        }
    }

    // POST /user/changePassword
    [HttpPost("changePassword")]
    public async Task<ApiResponse<object>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId=HttpContext.GetUserId();
            
            // 更新密码
            await _userService.UpdatePassword(userId, request.NewPassword, request.CurrentPassword);
            
            return ApiResponse<object>.Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail(ex.Message);
        }
    }

    // POST /user/uploadAvatar
    [HttpPost("uploadAvatar")]
    public async Task<ApiResponse<object>> UploadAvatar([FromForm] UploadAvatarRequest request)
    {
        try
        {
            var userId=HttpContext.GetUserId();
            
            // 验证文件大小 (限制为2MB)
            if (request.File.Length > 2 * 1024 * 1024)
            {
                return ApiResponse<object>.Fail("Avatar image size must be less than 2MB");
            }
            var stream = request.File.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] file = memoryStream.ToArray();

            // 上传头像
            var avatarId = await _imageBedService.UploadImageAsync(file, userId, "avatar");
            
            // 更新用户头像ID
            var user = await _userService.GetUserByIdAsync(userId);
            if (user != null)
            {
                user.Avatar = avatarId;
                await _userService.UpdateUserAvater(userId, avatarId);
            }
            
            return ApiResponse<object>.Ok(new UploadAvatarResultDto { AvatarId = avatarId });
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail(ex.Message);
        }
    }

    // POST /user/topup
    [HttpPost("topup")]
    public async Task<ApiResponse<object>> Topup([FromBody] TopUpRequest request)
    {
        try
        {
            // 从HttpContext中获取用户ID
            if (HttpContext.Items["UserId"] is not long userId)
            {
                return ApiResponse<object>.Fail("User not authenticated");
            }
            
            // 验证充值金额
            if (request.Amount <= 0)
            {
                return ApiResponse<object>.Fail("Top-up amount must be positive");
            }
            
            // 获取或创建用户资产记录
        UserAsset? asset;
        try
        {
            asset = await _userAssetService.GetUserAssetByUserIdAsync(userId);
            if (asset == null)
            {
                return ApiResponse.Fail("User asset not found");
            }
            else
            {
                // 增加用户余额 - AddToUserBalanceAsync 方法已经更新了总资产
                await _userAssetService.AddToUserBalanceAsync(userId, request.Amount);
            }
        }
            catch (Exception)
            {
                // 如果获取或更新失败，尝试创建新记录
                asset = await _userAssetService.CreateUserAssetAsync(userId, request.Amount);
            }
            
            return ApiResponse.Ok();
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail(ex.Message);
        }
    }
    
    // POST /user/openAccount
    [HttpPost("openAccount")]
    public async Task<ApiResponse> OpenAccount()
    {
        try
        {
            // 从HttpContext中获取用户ID
            if (HttpContext.Items["UserId"] is not long userId)
            {
                return ApiResponse.Fail("User not authenticated");
            }
            
            // 检查用户是否已经有资产账户
            var existingAsset = await _userAssetService.GetUserAssetByUserIdAsync(userId);
            if (existingAsset != null)
            {
                return ApiResponse.Fail("User already has an account");
            }
            
            // 创建新的用户资产账户
            var asset = await _userAssetService.CreateUserAssetAsync(userId, 0);
            
            return ApiResponse.Ok();
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail(ex.Message);
        }
    }
}
