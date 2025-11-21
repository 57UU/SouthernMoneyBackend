using Database;
using Microsoft.EntityFrameworkCore;
using SouthernMoneyBackend.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 加载敏感配置文件
if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Secrets.json")))
{
    builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);
}

// 注入依赖

builder.Services.AddControllers();
builder.Services.AddOpenApi();

//add dbcontext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<ImageBedService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); //add openapi support
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1"); // openapi doc
    });
}

app.UseAuthorization();

// 添加异常处理中间件
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthMiddleware();

app.MapControllers();

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
