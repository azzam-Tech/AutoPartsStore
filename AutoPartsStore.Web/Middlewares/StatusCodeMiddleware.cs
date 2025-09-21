using AutoPartsStore.Core.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AutoPartsStore.Web.Middlewares
{
    public class StatusCodeMiddleware
    {
        private readonly RequestDelegate _next;

        public StatusCodeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // بعد تنفيذ باقي الـ pipeline، نتحقق من الكود
            if (context.Response.HasStarted)
                return;

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(ApiResponse.FailureResult("غير مصرح. يرجى تسجيل الدخول."));
            }
            else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(ApiResponse.FailureResult("ممنوع الوصول. ليس لديك الصلاحيات الكافية."));
            }
        }
    }
}
