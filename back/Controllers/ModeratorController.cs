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

    // GET /api/moderator/applications?pendingOnly=true
    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications([FromQuery] bool pendingOnly = true)
    {
        var result = pendingOnly
            ? await _service.GetPendingApplicationsAsync()
            : await _service.GetAllApplicationsAsync();
        return Ok(result);
    }

    // PATCH /api/moderator/applications/{id}
    [HttpPatch("applications/{id}")]
    public async Task<IActionResult> ReviewApplication(Guid id, [FromBody] ReviewApplicationRequest request)
    {
        await _service.ReviewApplicationAsync(id, request);
        return Ok(new { message = "Статус заявки обновлён" });
    }

    // POST /api/moderator/users
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

    // PATCH /api/moderator/couriers/{id}/block — ручная блокировка без страйков
    [HttpPatch("couriers/{id}/block")]
    public async Task<IActionResult> BlockCourier(Guid id, [FromBody] BlockRequest request)
    {
        await _service.BlockCourierAsync(id, request);
        return Ok(new { message = request.IsBlocked ? "Курьер заблокирован" : "Курьер разблокирован" });
    }

    // POST /api/moderator/couriers/{id}/strike — добавить страйк (3 страйка = автоблок)
    [HttpPost("couriers/{id}/strike")]
    public async Task<IActionResult> AddCourierStrike(Guid id, [FromBody] AddStrikeRequest request)
    {
        await _service.AddCourierStrikeAsync(id, request);
        return Ok(new { message = "Страйк добавлен" });
    }

    // DELETE /api/moderator/couriers/{id}/fire — уволить курьера (soft delete + blacklist)
    [HttpDelete("couriers/{id}/fire")]
    public async Task<IActionResult> FireCourier(Guid id, [FromBody] FireCourierRequest request)
    {
        await _service.FireCourierAsync(id, request);
        return Ok(new { message = "Курьер уволен и добавлен в чёрный список" });
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

    // POST /api/moderator/organizations/{id}/strike
    [HttpPost("organizations/{id}/strike")]
    public async Task<IActionResult> AddOrgStrike(Guid id, [FromBody] AddStrikeRequest request)
    {
        await _service.AddOrgStrikeAsync(id, request);
        return Ok(new { message = "Страйк добавлен" });
    }

    // GET /api/moderator/orders
    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders()
    {
        var result = await _service.GetAllOrdersAsync();
        return Ok(result);
    }
}
