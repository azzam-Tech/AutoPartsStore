using AutoPartsStore.Core.Entities;
using AutoPartsStore.Infrastructure.Data;
using AutoPartsStore.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/emergency")]
public class EmergencyController : BaseController
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmergencyController> _logger;

    public EmergencyController(AppDbContext context, ILogger<EmergencyController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("create-admin")]
    [AllowAnonymous] // يسمح بالوصول بدون authentication
    public async Task<IActionResult> CreateEmergencyAdmin([FromBody] EmergencyAdminRequest request)
    {
        // تحقق من وجود مفتاح طوارئ سري (يمكن تخزينه في environment variables)
        var emergencyKey = Environment.GetEnvironmentVariable("EMERGENCY_ADMIN_KEY");

        if (emergencyKey == null || request.EmergencyKey != emergencyKey)
        {
            _logger.LogWarning("محاولة غير مصرحة لإنشاء مسؤول طوارئ");
            return Unauthorized("غير مصرح");
        }

        // تحقق إذا يوجد أي مسؤولين بالفعل
        var existingAdmins = await _context.UserRoleAssignments
            .Where(ura => ura.RoleId == 1) // Admin role
            .AnyAsync();

        if (existingAdmins)
        {
            return BadRequest("يوجد مسؤولون بالفعل في النظام");
        }

        // إنشاء المسؤول الجديد
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var adminUser = new User(
            request.Username,
            hashedPassword,
            request.Email,
            request.FullName,
            request.PhoneNumber
        );

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        // منح دور Admin
        var adminAssignment = new UserRoleAssignment(adminUser.Id, 1); // Admin role
        _context.UserRoleAssignments.Add(adminAssignment);
        await _context.SaveChangesAsync();

        _logger.LogCritical($"تم إنشاء مسؤول طوارئ: {request.Username}");

        return Success("تم إنشاء المسؤول بنجاح");
    }
}

public class EmergencyAdminRequest
{
    public string EmergencyKey { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
}