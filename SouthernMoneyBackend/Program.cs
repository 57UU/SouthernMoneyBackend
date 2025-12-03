using Database;
using Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SouthernMoneyBackend.Middleware;
using System.Text.Json;

// 配置Web应用程序 创建host 启动kestrel服务器
var builder = WebApplication.CreateBuilder(args);

// 加载敏感配置文件
if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Secrets.json")))
{
    builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);
}

// 注入依赖

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // 禁用小写转换
});  // 将使用控制器写web api 整套功能注入到应用
builder.Services.AddOpenApi();  // 注册生成openapi/swagger文档的服务

// 配置CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 尝试连接PostgreSQL，失败则回退到SQLite
// 检查命令行参数中是否包含 --use-pg
bool forceUsePostgres = args.Contains("--use-pg");
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
bool usePostgres = forceUsePostgres && Utils.IsPostgreSqlAvailable(connectionString);

if (usePostgres)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Program>();
    if (forceUsePostgres)
    {
        logger.LogInformation("检测到 --use-pg 参数，尝试使用 PostgreSQL 数据库");
    }
    else
    {
        logger.LogInformation("未指定 --use-pg 参数，使用 SQLite 数据库");
    }
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source=data.db"));
}

// 注入数据访问层
builder.Services.AddScoped<Database.Repositories.UserRepository>();
builder.Services.AddScoped<Database.Repositories.PostRepository>();
builder.Services.AddScoped<Database.Repositories.ImageRepository>();
builder.Services.AddScoped<Database.Repositories.ProductRepository>();
builder.Services.AddScoped<Database.Repositories.TransactionRepository>();
builder.Services.AddScoped<Database.Repositories.UserAssetRepository>();
builder.Services.AddScoped<Database.Repositories.ProductCategoryRepository>();
builder.Services.AddScoped<Database.Repositories.UserFavoriteCategoryRepository>();

// 注入业务逻辑层
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<ImageBedService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<UserAssetService>();
builder.Services.AddScoped<ProductCategoryService>();
builder.Services.AddScoped<UserFavoriteCategoryService>();


// 对上面配置好的服务 构建真正实例app
var app = builder.Build();
bool isDevEnv = app.Environment.IsDevelopment();

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
// 只在开发环境启用swagger ui
if (isDevEnv)
{
    string js=await File.ReadAllTextAsync("Utils/swaggerInject.js");
    app.MapOpenApi(); //add openapi support
    app.MapGet("/swagger-js", () => js);
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1"); // openapi doc
        options.InjectJavascript("/swagger-js");
    });
    //register test user 
    using (var scope = app.Services.CreateScope())
    {
        var userService=scope.ServiceProvider.GetService<UserService>()!;
        long userId=await userService.RegisterUser(new User { Name="test",Password="123"}, existIsOk: true);
        var adminService=scope.ServiceProvider.GetService<AdminService>()!;
        await adminService.SetAdmin(userId, alreadyAdminOk: true);
    }
    
    

}


//Note:中间件的顺序不要弄错
// 添加CORS中间件
app.UseCors("AllowAll");

// 添加异常处理中间件
app.UseMiddleware<ExceptionHandlerMiddleware>(new ExceptionHandlerMiddlewareOptions
{
    IncludeExceptionDetailsInProduction = isDevEnv,
});

// 添加认证中间件
app.UseAuthMiddleware(builder =>
{
    builder.Enable = !isDevEnv; //disable auth in dev env
});

// 授权中间件
app.UseAuthorization();

// 路由匹配到控制器
app.MapControllers();



// 运行应用 监听端口等待请求
app.Run();
