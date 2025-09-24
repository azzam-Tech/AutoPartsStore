using AutoPartsStore.Core.Exceptions;
using AutoPartsStore.Core.Models;
using System.Text.Json;

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
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);

            // تأكد أن الـ Response لم يبدأ بعد
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started, cannot handle exception middleware.");
                return;
            }

            var statusCode = ex switch
            {
                BusinessException => StatusCodes.Status400BadRequest,
                ValidationException => StatusCodes.Status400BadRequest,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var message = ex switch
            {
                BusinessException => "خطأ في منطق العمل",
                ValidationException => "خطأ في التحقق من البيانات",
                UnauthorizedException => "غير مصرح بالدخول",
                ForbiddenException => "الوصول ممنوع",
                NotFoundException => "العنصر غير موجود",
                _ => "خطأ غير متوقع"
            };

            var errors = ex is ValidationException vex ? vex.Errors : new List<string> { ex.Message };

            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors
            };

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = statusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}