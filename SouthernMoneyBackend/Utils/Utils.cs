public static class HttpContextExtensions
{
    public static Nullable<long> GetUserId(this HttpContext httpContext)
    {
        return (Nullable<long>)httpContext.Items["UserId"]!;
    }
    
    public static bool IsAdmin(this HttpContext httpContext)
    {
        return (bool)httpContext.Items["IsAdmin"]!;
    }
}