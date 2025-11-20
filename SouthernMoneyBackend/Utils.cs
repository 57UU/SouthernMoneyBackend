public static class HttpContextExtensions
{
    public static long GetUserId(this HttpContext httpContext)
    {
        return (long)httpContext.Items["UserId"]!;
    }
}