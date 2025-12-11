using Database;
using Service;
using SouthernMoneyBackend.Controllers;

namespace SouthernMoneyBackend;

public class AddDefault
{
    UserService userService;
    ImageBedService imageBedService;
    AdminService adminService;
    public  AddDefault(UserService userService,ImageBedService imageBedService,AdminService adminService)
    {
        this.userService = userService;
        this.imageBedService = imageBedService;
        this.adminService = adminService;
    }
    public async Task run()
    {
        long userId = await userService.RegisterUser(User.CreateUser("test", "123", 114514), existIsOk: true);
        await adminService.SetAdmin(userId, true, alreadyOk: true);
        //load image

        var img = await File.ReadAllBytesAsync("images/default_user.png");
        ImageBedController.defaultImage = img;
    }

}
