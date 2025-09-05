using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoPartsStore.Web.Middleware
{
    public class AdminCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminCheckMiddleware> _logger;

        public AdminCheckMiddleware(RequestDelegate next, ILogger<AdminCheckMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            // القائمة البيضاء للـ endpoints المسموحة بدون مسؤولين
            var allowedPaths = new[]
            {
                "/api/emergency",
                "/health",
                "/swagger",
                "/favicon.ico",
                "/api/auth/login"
            };

            if (allowedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                await _next(context);
                return;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    // التحقق من وجود مسؤولين نشطين
                    var hasActiveAdmins = await dbContext.UserRoleAssignments
                        .Include(ura => ura.Role)
                        .Include(ura => ura.User)
                        .Where(ura => ura.Role.RoleName == "Admin" &&
                                     ura.User.IsActive &&
                                     !ura.User.IsDeleted)
                        .AnyAsync();

                    if (!hasActiveAdmins)
                    {
                        _logger.LogCritical("⚠️ النظام غير مهيء: لا يوجد مسؤولين نشطين");

                        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            Success = false,
                            Message = "النظام غير مهيء. لا يوجد مسؤولين نشطين.",
                            Solution = new
                            {
                                Method = "POST",
                                Endpoint = "/api/emergency/create-admin",
                                Body = new
                                {
                                    emergencyKey = "مفتاح الطوارئ السري",
                                    username = "اسم المستخدم",
                                    password = "كلمة المرور",
                                    email = "البريد الإلكتروني"
                                }
                            }
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في التحقق من حالة النظام");
                    // في حالة الخطأ، نسمح للطلب بالمتابعة
                }
            }

            await _next(context);
        }
    }
}