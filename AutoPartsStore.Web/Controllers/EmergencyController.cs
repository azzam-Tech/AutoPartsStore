using AutoPartsStore.Core.Entities;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/emergency")]
    public class EmergencyController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmergencyController> _logger;
        private readonly IConfiguration _configuration;

        public EmergencyController(
            AppDbContext context,
            ILogger<EmergencyController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("create-admin")]
        [AllowAnonymous] // يسمح بالوصول بدون authentication
        public async Task<IActionResult> CreateEmergencyAdmin([FromBody] EmergencyAdminRequest request)
        {
            try
            {
                _logger.LogInformation("محاولة إنشاء مسؤول طوارئ من IP: {RemoteIpAddress}",
                    HttpContext.Connection.RemoteIpAddress);

                // القراءة من Environment Variables أولاً، ثم من Configuration
                var emergencyKey = Environment.GetEnvironmentVariable("EMERGENCY_ADMIN_KEY")
                                  ?? _configuration["EmergencySettings:EMERGENCY_ADMIN_KEY"];

                if (string.IsNullOrEmpty(emergencyKey))
                {
                    _logger.LogCritical("مفتاح الطوارئ غير مضبوط في النظام");
                    return StatusCode(500, "نظام الطوارئ غير مهيء. يرجى الاتصال بالدعم.");
                }

                if (request.EmergencyKey != emergencyKey)
                {
                    _logger.LogWarning("مفتاح طوارئ غير صحيح من IP: {RemoteIpAddress}",
                        HttpContext.Connection.RemoteIpAddress);
                    return Unauthorized("مفتاح طوارئ غير صحيح");
                }

                // التحقق من صحة البيانات
                if (string.IsNullOrEmpty(request.FullName) ||
                    string.IsNullOrEmpty(request.PhoneNumber) ||
                    string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest("اسم المستخدم او كلمة المرور او الايميل ليس صحيحا");
                }

                if (!request.Email.Contains("@"))
                {
                    return BadRequest("البريد الإلكتروني غير صحيح");
                }
                if (!request.Email.Contains("@gmail.com"))
                {
                    return BadRequest("يجب ان يكون بريد الكتروني من قوقل");
                }


                // تحقق إذا يوجد أي مسؤولين بالفعل
                var existingAdmins = await _context.UserRoleAssignments
                    .Include(ura => ura.Role)
                    .Where(ura => ura.Role.RoleName == "Admin")
                    .AnyAsync();

                if (existingAdmins)
                {
                    _logger.LogWarning("محاولة إنشاء مسؤول طوارئ مع وجود مسؤولين بالفعل");
                    return BadRequest("يوجد مسؤولون بالفعل في النظام");
                }

                // التحقق من عدم وجود مستخدم بنفس البيانات
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                {
                    _logger.LogWarning("محاولة إنشاء مسؤول ببيانات موجودة مسبقاً: {Email}", request.Email);
                    return BadRequest("اسم المستخدم أو البريد الإلكتروني مسجل مسبقاً");
                }


                // إنشاء المسؤول الجديد

                var adminUser = new User(
                    request.Email,
                    request.FullName ?? "مسؤول الطوارئ",
                    request.PhoneNumber ?? "0000000000"
                );

                adminUser.Activate();
                adminUser.Restore();

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();

                // البحث عن دور Admin أو إنشائه إذا لم exists
                var adminRole = await _context.UserRoles
                    .FirstOrDefaultAsync(r => r.RoleName == "Admin");

                if (adminRole == null)
                {
                    adminRole = new UserRole("Admin", "مسؤول النظام");
                    _context.UserRoles.Add(adminRole);
                    await _context.SaveChangesAsync();
                }

                // منح دور Admin
                var adminAssignment = new UserRoleAssignment(adminUser.Id, adminRole.Id);
                _context.UserRoleAssignments.Add(adminAssignment);
                await _context.SaveChangesAsync();

                _logger.LogCritical("تم إنشاء مسؤول طوارئ بنجاح: {Username} ({Email})",
                    request.FullName, request.Email);

                return Success(new
                {
                    Email = adminUser.Email,
                    UserId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow
                }, "تم إنشاء المسؤول بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ غير متوقع أثناء إنشاء مسؤول الطوارئ");
                return StatusCode(500, "حدث خطأ داخلي أثناء إنشاء المسؤول");
            }
        }

        [HttpGet("system-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSystemStatus()
        {
            try
            {
                var adminCount = await _context.UserRoleAssignments
                    .Include(ura => ura.Role)
                    .Where(ura => ura.Role.RoleName == "Admin")
                    .CountAsync();

                var totalUsers = await _context.Users.CountAsync();

                return Success(new
                {
                    HasAdmins = adminCount > 0,
                    AdminCount = adminCount,
                    TotalUsers = totalUsers,
                    ServerTime = DateTime.UtcNow,
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                }, "حالة النظام");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من حالة النظام");
                return StatusCode(500, "لا يمكن التحقق من حالة النظام");
            }
        }
    }

    public class EmergencyAdminRequest
    {
        public string EmergencyKey { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}