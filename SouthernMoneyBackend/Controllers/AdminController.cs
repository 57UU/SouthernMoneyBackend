using Microsoft.AspNetCore.Mvc;
using Service;
using SouthernMoneyBackend.Utils;
using Microsoft.AspNetCore.Authorization;
using SouthernMoneyBackend.Middleware;

namespace SouthernMoneyBackend.Controllers;

[ApiController]
[Route("/admin")]
[AuthorizeAdmin]
public class AdminController : ControllerBase
{
    private readonly AdminService adminService;

    public AdminController(AdminService adminService)
    {
        this.adminService = adminService;
    }

    /// <summary>
    /// 封禁 / 解封 用户
    /// </summary>
    [HttpPost("/handleUser")]
    public async Task<ApiResponse<object>> HandleUserAsync([FromBody] HandleUserRequest req)
    {
        if (req == null)
        {
            return ApiResponse.Fail("Invalid request", "INVALID_REQUEST");
        }

        if (req.UserId <= 0)
        {
            return ApiResponse.Fail("Invalid userId", "INVALID_USER_ID");
        }

        try
        {
            if (req.IsBlocked)
            {
                await adminService.BanUser(req.UserId, req.HandleReason ?? "No reason");
            }
            else
            {
                await adminService.UnbanUser(req.UserId);
            }

            return ApiResponse.Ok(req.IsBlocked);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail(ex.Message, "HANDLE_USER_FAILED");
        }
    }
}
