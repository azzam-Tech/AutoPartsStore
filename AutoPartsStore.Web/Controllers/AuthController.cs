using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(
            request.Username,
            request.Email,
            request.FullName,
            request.PhoneNumber,
            request.Password);

        if (!result.Success)
            return BadRequest(result.Message);

        return Success(result.Message);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var isValid = await _authService.ValidateCredentialsAsync(request.Username, request.Password);
        if (!isValid)
            return Unauthorized("بيانات الدخول غير صحيحة.");

        var tokenResult = await _authService.GenerateJwtTokenAsync(request.Username);
        if (!tokenResult.Success)
            return BadRequest(tokenResult.Message);

        // إرجاع كائن موحد يحتوي على كل شيء
        return Success(new
        {
            accessToken = tokenResult.AccessToken, 
            expiresAt = tokenResult.ExpiresAt,     
            userInfo = tokenResult.UserInfo        
        }, tokenResult.Message);
    }
}

// نماذج Request
public class RegisterRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
}

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}