using System.Net;
using System.Text.Json;
using AutoPartsStore.Core.Exceptions;
using AutoPartsStore.Core.Models; // ApiResponse

namespace AutoPartsStore.Web.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // مرر الطلب لبقية الـ Middleware
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                BusinessException => (int)HttpStatusCode.BadRequest,              // خطأ في منطق العمل
                ValidationException => (int)HttpStatusCode.BadRequest,            // خطأ في التحقق من البيانات
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,        // غير مصرح
                ForbiddenException => (int)HttpStatusCode.Forbidden,              // ممنوع الوصول
                NotFoundException => (int)HttpStatusCode.NotFound,                // غير موجود
                _ => (int)HttpStatusCode.InternalServerError                      // أي خطأ آخر
            };


            ApiResponse response = ex switch
            {
                BusinessException bex => ApiResponse.FailureResult("خطأ في منطق العمل", new List<string> { bex.Message }),
                ValidationException vex => ApiResponse.FailureResult("خطأ في التحقق من البيانات", new List<string> { vex.Message }),
                UnauthorizedException => ApiResponse.FailureResult("غير مصرح بالدخول", new List<string> { ex.Message }),
                ForbiddenException => ApiResponse.FailureResult("الوصول ممنوع", new List<string> { ex.Message }),
                NotFoundException => ApiResponse.FailureResult("العنصر غير موجود", new List<string> { ex.Message }),
                _ => ApiResponse.FailureResult("خطأ غير متوقع", new List<string> { ex.Message })
            };


            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var result = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(result);
        }
    }
}
