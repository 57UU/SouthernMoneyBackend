public static class HttpContextExtensions
{
    public static long GetUserId(this HttpContext httpContext)
    {
        return (long)httpContext.Items["UserId"]!;
    }
    
    public static bool IsAdmin(this HttpContext httpContext)
    {
        return (bool)httpContext.Items["IsAdmin"]!;
    }
}
