using System.Security.Claims;
using DeliveryAggregator.DTOs;
using DeliveryAggregator.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAggregator.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // POST /api/auth/login
    // Возвращает IsEmailConfirmed — фронт показывает предупреждение если false
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        return Ok(result);
    }

    // POST /api/auth/register
    // Только покупатели. Курьеры/владельцы — через /api/applications
    // После регистрации отправляется письмо подтверждения
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest request)
    {
        var result = await _auth.RegisterCustomerAsync(request);
        return Ok(result);
    }

    // GET /api/auth/confirm-email?token=xxx
    // Ссылка из письма — подтверждает email
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        var success = await _auth.ConfirmEmailAsync(token);
        if (!success)
            return BadRequest(new { error = "Ссылка недействительна или истекла" });

        return Ok(new { message = "Email успешно подтверждён" });
    }

    // POST /api/auth/resend-confirmation
    // Повторно отправить письмо подтверждения
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request)
    {
        await _auth.ResendConfirmationAsync(request.Email);
        return Ok(new { message = "Письмо отправлено" });
    }

    // POST /api/auth/change-password
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        await _auth.ChangePasswordAsync(UserId, request);
        return Ok(new { message = "Пароль успешно изменён" });
    }
}
