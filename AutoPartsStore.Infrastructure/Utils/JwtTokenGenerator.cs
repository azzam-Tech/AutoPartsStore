using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.AuthModels;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoPartsStore.Infrastructure.Utils
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public JwtTokenGenerator(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        public async Task<AuthenticationResult> GenerateToken(string userId, string email, string fullName)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return AuthenticationResult.FailureResult("المستخدم غير موجود.");

            var userRoles = await _userService.GetUserRolesAsync(int.Parse(userId));
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = jwtSettings["Key"] ?? _configuration["JWT_KEY"] ?? throw new InvalidOperationException("JWT Key is not configured."); ;
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryInDays = 60;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
             new Claim(JwtRegisteredClaimNames.Sub, userId),
             new Claim(JwtRegisteredClaimNames.Email, email),
             new Claim("FullName", fullName),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in userRoles)
            {
                claims = claims.Append(new Claim(ClaimTypes.Role, role.RoleName)).ToArray();
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expiryInDays),
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return AuthenticationResult.SuccessResult(
               message: "تم تسجيل الدخول بنجاح",
               accessToken: accessToken,
               expiresAt: token.ValidTo,
               userInfo: new UserInfoDto
               {
                   Id = user.Id,
                   Email = user.Email,
                   FullName = user.FullName,
                   PhoneNumber = user.PhoneNumber,
                   Roles = userRoles.Select(r => r.RoleName).ToList(),
                   RedirectTo = DetermineRedirectPath(userRoles),
                   SessionInfo = new SessionInfoDto
                   {
                       LastLogin = user.LastLoginDate
                   }
               }
           );
        }


        private string DetermineRedirectPath(List<UserRole> roles)
        {
            if (roles.Any(r => r.RoleName == "Admin"))
                return "/admin/dashboard";

            if (roles.Any(r => r.RoleName == "Supplier"))
                return "/supplier/products";

            if (roles.Any(r => r.RoleName == "Customer"))
                return "/store";

            return "/";
        }
    }
}
