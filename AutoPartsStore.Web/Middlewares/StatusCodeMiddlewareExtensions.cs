using Microsoft.AspNetCore.Builder;

namespace AutoPartsStore.Web.Middlewares
{
    public static class StatusCodeMiddlewareExtensions
    {
        /// <summary>
        /// يُستخدم لتحويل أكواد الحالة مثل 401 Unauthorized و 403 Forbidden إلى ردود JSON موحدة
        /// باستخدام نموذج ApiResponse.
        /// يجب استخدامه بعد UseAuthentication و UseAuthorization.
        /// </summary>
        /// <param name="builder">ApplicationBuilder</param>
        /// <returns>IApplicationBuilder</returns>
        public static IApplicationBuilder UseStatusCodeHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<StatusCodeMiddleware>();
        }
    }
}
