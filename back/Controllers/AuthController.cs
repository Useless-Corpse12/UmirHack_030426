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
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        return Ok(result);
    }

    // POST /api/auth/register
    // Только покупатели регистрируются сами.
    // Курьеры и владельцы - через /api/applications
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest request)
    {
        var result = await _auth.RegisterCustomerAsync(request);
        return Ok(result);
    }

    // POST /api/auth/change-password
    // Принимает токен + старый пароль + новый пароль, если токен валиден и старый пароль верный — меняет
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        await _auth.ChangePasswordAsync(UserId, request);
        return Ok(new { message = "Пароль успешно изменён" });
    }
}
