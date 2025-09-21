using Microsoft.AspNetCore.Builder;

namespace AutoPartsStore.Web.Middlewares
{
    public static class StatusCodeMiddlewareExtensions
    {
        public static IApplicationBuilder UseStatusCodeHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<StatusCodeMiddleware>();
        }
    }
}
