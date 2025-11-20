using Microsoft.AspNetCore.Mvc;
using Database;

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
        try{
            await userService.RegisterUser(user);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        return Ok();
    }
    [HttpPost("loginByPassword", Name = "LoginByPassword")]
    public async Task<IActionResult> LoginByPassword([FromBody] LoginByPasswordRequest request)
    {
        try{
            var session = await userService.LoginByPassword(request.Name, request.Password);
            return Ok(session);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    [HttpPost("loginByToken", Name = "LoginByToken")]
    public async Task<IActionResult> LoginByToken([FromBody] LoginByTokenRequest request)
    {
        try{
            var session = await userService.LoginByToken(request.Token);
            return Ok(session);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

}
