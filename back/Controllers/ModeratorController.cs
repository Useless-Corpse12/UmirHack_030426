using DeliveryAggregator.DTOs;
using DeliveryAggregator.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAggregator.Controllers;

[ApiController]
[Route("api/moderator")]
[Authorize(Roles = "Moderator")]
public class ModeratorController : ControllerBase
{
    private readonly IModeratorService _service;

    public ModeratorController(IModeratorService service) => _service = service;

    // GET /api/moderator/applications
    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications([FromQuery] bool pendingOnly = true)
    {
        var result = pendingOnly
            ? await _service.GetPendingApplicationsAsync()
            : await _service.GetAllApplicationsAsync();
        return Ok(result);
    }

    // PATCH /api/moderator/applications/{id}
    // Одобрить или отклонить заявку
    [HttpPatch("applications/{id}")]
    public async Task<IActionResult> ReviewApplication(Guid id, [FromBody] ReviewApplicationRequest request)
    {
        await _service.ReviewApplicationAsync(id, request);
        return Ok(new { message = "Статус заявки обновлён" });
    }

    // POST /api/moderator/users
    // Создать пользователя (курьера или владельца орги) и выслать ему логин/пароль
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await _service.CreateUserAsync(request);
        return Ok(result);
    }

    // GET /api/moderator/couriers
    [HttpGet("couriers")]
    public async Task<IActionResult> GetCouriers()
    {
        var result = await _service.GetAllCouriersAsync();
        return Ok(result);
    }

    // PATCH /api/moderator/couriers/{id}/block
    [HttpPatch("couriers/{id}/block")]
    public async Task<IActionResult> BlockCourier(Guid id, [FromBody] BlockRequest request)
    {
        await _service.BlockCourierAsync(id, request);
        return Ok(new { message = request.IsBlocked ? "Курьер заблокирован" : "Курьер разблокирован" });
    }

    // GET /api/moderator/organizations
    [HttpGet("organizations")]
    public async Task<IActionResult> GetOrganizations()
    {
        var result = await _service.GetAllOrganizationsAsync();
        return Ok(result);
    }

    // PATCH /api/moderator/organizations/{id}/block
    [HttpPatch("organizations/{id}/block")]
    public async Task<IActionResult> BlockOrganization(Guid id, [FromBody] BlockRequest request)
    {
        await _service.BlockOrganizationAsync(id, request);
        return Ok(new { message = request.IsBlocked ? "Организация заблокирована" : "Организация разблокирована" });
    }

    // GET /api/moderator/orders
    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders()
    {
        var result = await _service.GetAllOrdersAsync();
        return Ok(result);
    }
}
