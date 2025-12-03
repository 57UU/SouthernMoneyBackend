using Database;
using Microsoft.EntityFrameworkCore;

public static class HttpContextExtensions
{
    public static long GetUserId(this HttpContext httpContext)
    {
        if(httpContext.Items.TryGetValue("UserId", out var userId))
        {
            return (long)userId!;
        }
        else
        {
            throw new Exception("User ID not found");
        }
    }

    public static bool IsAdmin(this HttpContext httpContext)
    {
        return (bool)httpContext.Items["IsAdmin"]!;
    }
}

static class Utils
{
    public static bool IsPostgreSqlAvailable(string? connectionString, int timeout = 3)
    {
        // 检查连接字符串是否为空
        if (string.IsNullOrEmpty(connectionString))
        {
            return false;
        }
        else
        {
            // 首先尝试PostgreSQL连接
            try
            {
                var postgresOptions = new DbContextOptionsBuilder<AppDbContext>()
                    .UseNpgsql($"{connectionString};Timeout={timeout}")
                    .Options;

                using (var testContext = new AppDbContext(postgresOptions))
                {
                    testContext.Database.CanConnect();
                    testContext.Database.EnsureCreated();
                }
                return true;
            }
            catch
            {
                // PostgreSQL连接失败
                return false;
            }
        }

    }
}