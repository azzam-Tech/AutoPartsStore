using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Interfaces.IServices.IEmailSirvices;
using AutoPartsStore.Core.Models.AuthModels;
using AutoPartsStore.Core.Models.AuthModels.EmailAuthModels;
using AutoPartsStore.Infrastructure.Data;
using AutoPartsStore.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace AutoPartsStore.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly JwtTokenGenerator _jwtTokenGenerator;


        public AuthService(AppDbContext context, IUserService userService, IConfiguration configuration, IEmailService emailService, JwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _userService = userService;
            _configuration = configuration;
            _emailService = emailService;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<bool> SendCode(SendCodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new InvalidOperationException("JWT Key is not configured.");

            var code = GenerateRandomCode();

            var user = await _userService.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                // مستخدم جديد — ننشئ سجل مؤقت
                User newUser = new User(email: request.Email, fullName: "", phoneNumber: "")
                {
                    VerificationCode = code,
                    VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(2),
                    IsEmailVerified = false
                };
                _context.Users.Add(newUser);
            }
            else
            {
                // مستخدم موجود — نحدث الكود فقط
                user.VerificationCode = code;
                user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(2);
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendVerificationCodeAsync(request.Email, code);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("فشل إرسال البريد: " + ex.Message);
            }
        }

        public async Task<AuthenticationResult> VerifyCode(VerifyCodeRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.VerificationCode == request.Code);

            if (user == null || user.VerificationCodeExpiry < DateTime.UtcNow)
                throw new InvalidOperationException("الرمز غير صحيح أو منتهي الصلاحية");

            // تحقق ناجح
            user.IsEmailVerified = true;
            user.VerificationCode = null; // نمسح الكود بعد الاستخدام
            user.VerificationCodeExpiry = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // إذا كان المستخدم لديه اسم كامل ورقم هاتف → دخول مباشر
            if (!string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                var result = _jwtTokenGenerator.GenerateToken(user.Id.ToString(), user.Email, user.FullName);

                return await result;
            }
            else
            {
                // يحتاج لإكمال التسجيل
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "التحقق ناجح. يرجى إكمال التسجيل.",
                    AccessToken = null,
                    ExpiresAt = null,
                    UserInfo = null
                };
            }
        }


        public async Task<AuthenticationResult> CompleteRegistration(CompleteRegistrationRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !user.IsEmailVerified)
                throw new InvalidOperationException("البريد غير مسجل أو غير مفعل");

            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new InvalidOperationException("الاسم ورقم الهاتف مطلوبان");

            user.UpdateUsre(user.Email, request.FullName, request.PhoneNumber);
            _context.Users.Update(user);

            var customerRole = await _context.UserRoles
              .FirstOrDefaultAsync(r => r.RoleName == "Customer");

            if (customerRole != null)
            {
                var assignment = new UserRoleAssignment(user.Id, customerRole.Id);
                _context.UserRoleAssignments.Add(assignment);
                await _context.SaveChangesAsync();
            }
            else
            {
                // إذا لم يكن دور Customer موجودًا، يمكننا إنشاؤه
                customerRole = new UserRole("Customer", "زبون المتجر");
                _context.UserRoles.Add(customerRole);
                await _context.SaveChangesAsync();
                var assignment = new UserRoleAssignment(user.Id, customerRole.Id);
                _context.UserRoleAssignments.Add(assignment);
                await _context.SaveChangesAsync();
            }

            var result = _jwtTokenGenerator.GenerateToken(user.Id.ToString(), user.Email, user.FullName);
            return await result;
        }


        //public async Task<AuthenticationResult> GenerateJwtTokenAsync(string username)
        //{
        //    var user = await _userService.GetUserByEmailAsync(username);
        //    if (user == null)
        //        return AuthenticationResult.FailureResult("المستخدم غير موجود.");

        //    var roles = await GetUserRolesAsync(user.Id);
        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //        new Claim(ClaimTypes.Email, user.Email),
        //        new Claim("FullName", user.FullName)
        //    };

        //    foreach (var role in roles)
        //    {
        //        claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
        //    }

        //    var key = _configuration["Jwt:Key"] ?? _configuration["JWT_KEY"]
        //             ?? throw new InvalidOperationException("JWT Key is not configured.");

        //    var issuer = _configuration["Jwt:Issuer"] ?? "AutoPartsStore.Api";
        //    var audience = _configuration["Jwt:Audience"] ?? "AutoPartsStore.Client";

        //    var keyBytes = Encoding.UTF8.GetBytes(key);
        //    var signingCredentials = new SigningCredentials(
        //        new SymmetricSecurityKey(keyBytes),
        //        SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(
        //        issuer: issuer,
        //        audience: audience,
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddDays(30),
        //        signingCredentials: signingCredentials);

        //    var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        //    return AuthenticationResult.SuccessResult(
        //        message: "تم تسجيل الدخول بنجاح",
        //        accessToken: accessToken,
        //        expiresAt: token.ValidTo,
        //        userInfo: new UserInfoDto
        //        {
        //            Id = user.Id,
        //            Email = user.Email,
        //            FullName = user.FullName,
        //            PhoneNumber = user.PhoneNumber,
        //            Roles = roles.Select(r => r.RoleName).ToList(),
        //            RedirectTo = DetermineRedirectPath(roles),
        //            SessionInfo = new SessionInfoDto
        //            {
        //                LastLogin = user.LastLoginDate
        //            }
        //        }
        //    );
        //}

        //public async Task<AuthenticationResult> RegisterAsync(
        //    string username,
        //    string email,
        //    string fullName,
        //    string phoneNumber,
        //    string password)
        //{

        //    if (await _userService.EmailExistsAsync(email))
        //    {
        //        return AuthenticationResult.FailureResult("البريد الإلكتروني مسجل مسبقًا.");
        //    }

        //    // تشفير كلمة المرور
        //    var hashedPassword = HashCode(password);

        //    // إنشاء المستخدم
        //    var user = new User(username, hashedPassword, email, fullName, phoneNumber);

        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();

        //    // ربط الدور: Customer
        //    var customerRole = await _context.UserRoles
        //        .FirstOrDefaultAsync(r => r.RoleName == "Customer");

        //    if (customerRole != null)
        //    {
        //        var assignment = new UserRoleAssignment(user.Id, customerRole.Id);
        //        _context.UserRoleAssignments.Add(assignment);
        //        await _context.SaveChangesAsync();
        //    }
        //    else 
        //    {
        //        // إذا لم يكن دور Customer موجودًا، يمكننا إنشاؤه
        //        customerRole = new UserRole("Customer", "زبون المتجر");
        //        _context.UserRoles.Add(customerRole);
        //        await _context.SaveChangesAsync();
        //        var assignment = new UserRoleAssignment(user.Id, customerRole.Id);
        //        _context.UserRoleAssignments.Add(assignment);
        //        await _context.SaveChangesAsync();
        //    }

        //    return AuthenticationResult.SuccessResult("تم التسجيل بنجاح.");
        //}




        //private async Task<List<UserRole>> GetUserRolesAsync(int userId)
        //{
        //    return await _context.UserRoleAssignments
        //        .Where(a => a.UserId == userId)
        //        .Include(a => a.Role)
        //        .Select(a => a.Role)
        //        .ToListAsync();
        //}

        private string HashCode(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyCode(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        private string GenerateRandomCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4]; // 4 بايت → رقم عشوائي كبير
            rng.GetBytes(bytes);
            var random = BitConverter.ToUInt32(bytes, 0) % 900000 + 100000; // بين 100000 و 999999
            return random.ToString();
        }
    }




}