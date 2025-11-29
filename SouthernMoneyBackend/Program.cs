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
builder.Services.AddOpenApi();  // 注册生成openai/swagger文档的服务

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
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
bool usePostgres = Utils.IsPostgreSqlAvailable(connectionString);

if (usePostgres)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Program>();
    logger.LogWarning("PostgreSQL连接失败，回退到SQLite数据库");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source=data.db"));
}

// 注入数据访问层
builder.Services.AddScoped<Database.Repositories.UserRepository>();
builder.Services.AddScoped<Database.Repositories.PostRepository>();
builder.Services.AddScoped<Database.Repositories.ImageRepository>();
// 注入业务逻辑层
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<ImageBedService>();


// 对上面配置好的服务 构建真正实例app
var app = builder.Build();
bool isDevEnv = app.Environment.IsDevelopment();

// Configure the HTTP request pipeline.
// 只在开发环境启用swagger ui
if (isDevEnv)
{
    app.MapOpenApi(); //add openapi support
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1"); // openapi doc
    });
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

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// 运行应用 监听端口等待请求
app.Run();
