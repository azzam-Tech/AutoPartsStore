using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoPartsStore.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IUserService userService, IConfiguration configuration)
        {
            _context = context;
            _userService = userService;
            _configuration = configuration;
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return false;

            return VerifyPassword(password, user.PasswordHash);
        }

        public async Task<AuthenticationResult> GenerateJwtTokenAsync(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return AuthenticationResult.FailureResult("المستخدم غير موجود.");

            var roles = await GetUserRolesAsync(user.Id);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
            }

            var key = _configuration["JWT_KEY"]
                     ?? throw new InvalidOperationException("JWT Key is not configured.");

            var issuer = _configuration["Jwt:Issuer"] ?? "AutoPartsStore.Api";
            var audience = _configuration["Jwt:Audience"] ?? "AutoPartsStore.Client";

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: signingCredentials);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return AuthenticationResult.SuccessResult(
                "تم إنشاء التوكن بنجاح",
                accessToken,
                token.ValidTo
            );
        }

        public async Task<AuthenticationResult> RegisterAsync(
            string username,
            string email,
            string fullName,
            string phoneNumber,
            string password)
        {
            // التحقق من التكرار
            if (await _userService.UsernameExistsAsync(username))
            {
                return AuthenticationResult.FailureResult("اسم المستخدم مسجل مسبقًا.");
            }

            if (await _userService.EmailExistsAsync(email))
            {
                return AuthenticationResult.FailureResult("البريد الإلكتروني مسجل مسبقًا.");
            }

            // تشفير كلمة المرور
            var hashedPassword = HashPassword(password);

            // إنشاء المستخدم
            var user = new User(username, hashedPassword, email, fullName, phoneNumber);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // ربط الدور: Customer
            var customerRole = await _context.UserRoles
                .FirstOrDefaultAsync(r => r.RoleName == "Customer");

            if (customerRole != null)
            {
                var assignment = new UserRoleAssignment(user.Id, customerRole.Id);
                _context.UserRoleAssignments.Add(assignment);
                await _context.SaveChangesAsync();
            }

            return AuthenticationResult.SuccessResult("تم التسجيل بنجاح.");
        }

        // ========================
        // دوال مساعدة داخلية
        // ========================

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
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

        private async Task<List<UserRole>> GetUserRolesAsync(int userId)
        {
            return await _context.UserRoleAssignments
                .Where(a => a.UserId == userId)
                .Include(a => a.Role)
                .Select(a => a.Role)
                .ToListAsync();
        }
    }
}