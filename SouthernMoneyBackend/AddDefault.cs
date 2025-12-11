using Database;
using Service;
using SouthernMoneyBackend.Controllers;

namespace SouthernMoneyBackend;

public class AddDefault
{
    UserService userService;
    ImageBedService imageBedService;
    AdminService adminService;
    ProductCategoryService productCategoryService;
    public AddDefault(
        UserService userService,
        ImageBedService imageBedService,
        AdminService adminService,
        ProductCategoryService productCategoryService
        )
    {
        this.userService = userService;
        this.imageBedService = imageBedService;
        this.adminService = adminService;
        this.productCategoryService = productCategoryService;
    }
    public readonly Guid DEFAULT_IMAGE= Guid.Parse("00000000-0000-0000-0000-000000000001");
    public readonly Guid FUTURES_CATEGORY = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public readonly Guid GOLD_CATEGORY = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public readonly Guid VIRTUAL_CURRENCY = Guid.Parse("00000000-0000-0000-0000-000000000004");
    public async Task run()
    {
        long userId = await userService.RegisterUser(User.CreateUser("test", "123", 114514), existIsOk: true);
        await adminService.SetAdmin(userId, true, alreadyOk: true);
        //load image

        var img = await File.ReadAllBytesAsync("images/default_user.png");
        ImageBedController.defaultImage = img;
        try
        {
            await imageBedService.UploadImageAsync(img, userId, "image/jpeg", DEFAULT_IMAGE);
        }
        catch (Exception) { }
        if(await productCategoryService.GetCategoryByIdAsync(FUTURES_CATEGORY) == null)
        {
            var futureImg = await File.ReadAllBytesAsync("images/oil.png");
            var imgId=await imageBedService.UploadImageAsync(futureImg, userId, "image/jpeg");
            await productCategoryService.CreateCategoryByGuidAsync("期货", imgId, FUTURES_CATEGORY);
        }
        if(await productCategoryService.GetCategoryByIdAsync(GOLD_CATEGORY) == null)
        {
            var goldImg = await File.ReadAllBytesAsync("images/gold.png");
            var imgId=await imageBedService.UploadImageAsync(goldImg, userId, "image/jpeg");
            await productCategoryService.CreateCategoryByGuidAsync("黄金", imgId, GOLD_CATEGORY);
        }
        if(await productCategoryService.GetCategoryByIdAsync(VIRTUAL_CURRENCY) == null)
        {
            var virtualImg = await File.ReadAllBytesAsync("images/dollar.png");
            var imgId=await imageBedService.UploadImageAsync(virtualImg, userId, "image/jpeg");
            await productCategoryService.CreateCategoryByGuidAsync("虚拟货币", imgId, VIRTUAL_CURRENCY);
        }


    }

}
