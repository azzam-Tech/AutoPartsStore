namespace AutoPartsStore.Web.Middleware
{
    public static class AdminCheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseAdminCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AdminCheckMiddleware>();
        }
    }
}