using Database;
using Microsoft.EntityFrameworkCore;
using SouthernMoneyBackend.Middleware;

// 配置Web应用程序 创建host 启动kestrel服务器
var builder = WebApplication.CreateBuilder(args);

// 加载敏感配置文件
if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Secrets.json")))
{
    builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);
}

// 注入依赖

builder.Services.AddControllers();  // 将使用控制器写web api 整套功能注入到应用
builder.Services.AddOpenApi();  // 注册生成openai/swagger文档的服务

//add dbcontext 定义数据库上下文创建规则
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 注入自定义服务
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<ImageBedService>();


// 对上面配置好的服务 构建真正实例app
var app = builder.Build();

// Configure the HTTP request pipeline.
// 只在开发环境启用swagger ui
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); //add openapi support
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1"); // openapi doc
    });
}

// 授权中间件
app.UseAuthorization();

// 添加异常处理中间件
app.UseMiddleware<ExceptionHandlerMiddleware>();

// 添加认证中间件
app.UseAuthMiddleware(builder=>{
    builder.Enable = !app.Environment.IsDevelopment(); //disable auth in dev env
});

// 路由匹配到控制器
app.MapControllers();

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

// 运行应用 监听端口等待请求
app.Run();
