using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.AuthModels;
using AutoPartsStore.Core.Models.AuthModels.EmailAuthModels;
using AutoPartsStore.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("send-code")]
    [EnableRateLimiting("SendCodeRateLimit")]

    public async Task<IActionResult> SendCode([FromBody] SendCodeRequest request)
    {
        var result = await _authService.SendCode(request);
        if (!result)
            return BadRequest("فشل في إرسال رمز التحقق. حاول مرة أخرى.");
        return Success("تم إرسال رمز التحقق إلى بريدك الإلكتروني.");
    }

    [HttpPost("verify-code")]
    [EnableRateLimiting("VerifyCodeRateLimit")]

    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        var result = await _authService.VerifyCode(request);
        if (!result.Success)
            return Success(new LoginInfo
            {
                login = false,
                accessToken = null,
                expiresAt = null,
                userInfo = null
            }, "يرحى استكمال عمبية انشاء الحساب");
        return Success(new LoginInfo
        {
            login = true,
            accessToken = result.AccessToken,
            expiresAt = result.ExpiresAt,
            userInfo = result.UserInfo
        }, "تم تسجيل الدخول بنجاح");

    }

    [HttpPost("complete-registration")]
    public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationRequest request)
    {
        var result = await _authService.CompleteRegistration(request);
        if (!result.Success)
            return BadRequest("حدث مشكلة اثناء انشاء الحساب");

        return Success(new LoginInfo
        {
            login = true,
            accessToken = result.AccessToken,
            expiresAt = result.ExpiresAt,
            userInfo = result.UserInfo
        }, "تم إنشاء الحساب وتسجيل الدخول بنجاح");
    }

}
