using AutoPartsStore.Core.Models;

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
            // احفظ الـ Stream الأصلي للـ Response
            var originalBodyStream = context.Response.Body;

            // استخدم MemoryStream لاعتراض الـ Response قبل إرساله
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context); // تنفيذ باقي Pipeline

            // تحقق من حالة الاستجابة
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized ||
                context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                // اقرأ ما كُتب في الـ Response Body حتى الآن
                var bodyText = await ReadStreamAsync(responseBody);

                // إذا كان الـ Body فارغ أو ليس JSON (غالباً HTML أو فارغ من ASP.NET Core)
                if (string.IsNullOrWhiteSpace(bodyText) || !IsJsonResponse(context))
                {
                    // أعد تعيين الـ Body للـ Stream الأصلي للكتابة عليه
                    context.Response.Body = originalBodyStream;

                    // امسح أي Headers سابقة لو وُجدت (خاصة Content-Length)
                    context.Response.Headers.Remove("Content-Length");

                    // عيّن النوع وابدأ الكتابة
                    context.Response.ContentType = "application/json; charset=utf-8";

                    var message = context.Response.StatusCode switch
                    {
                        StatusCodes.Status401Unauthorized => "غير مصرح. يرجى تسجيل الدخول.",
                        StatusCodes.Status403Forbidden => "ممنوع الوصول. ليس لديك الصلاحيات الكافية.",
                        _ => "حدث خطأ في التحقق من الصلاحيات."
                    };

                    var response = ApiResponse.FailureResult(message);

                    await context.Response.WriteAsJsonAsync(response);
                    return;
                }
            }

            // إذا لم يكن هناك تعديل، أعد كتابة الـ Body الأصلي
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private async Task<string> ReadStreamAsync(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        private bool IsJsonResponse(HttpContext context)
        {
            var contentType = context.Response.ContentType;
            return !string.IsNullOrEmpty(contentType) &&
                   contentType.Contains("application/json", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}