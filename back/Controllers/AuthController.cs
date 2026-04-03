using DeliveryAggregator.DTOs;
using DeliveryAggregator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAggregator.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

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
}
