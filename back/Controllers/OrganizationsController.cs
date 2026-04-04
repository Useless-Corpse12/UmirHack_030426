using System.Security.Claims;
using DeliveryAggregator.DTOs;
using DeliveryAggregator.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAggregator.Controllers;

[ApiController]
[Route("api/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _service;

    public OrganizationsController(IOrganizationService service) => _service = service;

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // GET /api/organizations
    // Список всех организаций для покупателя (публичный)
    [HttpGet]
    public async Task<IActionResult> GetAllOrganizations()
    {
        var result = await _service.GetAllOrganizationsAsync();
        return Ok(result);
    }

    // GET /api/organizations/{orgId}/restaurants
    // Рестораны конкретной организации — покупатель выбрал оргу, смотрит точки
    [HttpGet("{orgId}/restaurants")]
    public async Task<IActionResult> GetRestaurantsByOrg(Guid orgId)
    {
        var result = await _service.GetRestaurantsByOrgAsync(orgId);
        return Ok(result);
    }

    // GET /api/organizations/restaurants
    // Все рестораны всех организаций (публичный)
    [HttpGet("restaurants")]
    public async Task<IActionResult> GetAllRestaurants()
    {
        var result = await _service.GetAllRestaurantsAsync();
        return Ok(result);
    }

    // GET /api/organizations/my
    // Владелец смотрит свою организацию с ресторанами
    [Authorize(Roles = "OrganizationOwner")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var result = await _service.GetMyOrganizationAsync(UserId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // POST /api/organizations/restaurants
    // Без геолокации (Lat/Lng) ресторан создаётся с IsActive = false автоматически
    [Authorize(Roles = "OrganizationOwner")]
    [HttpPost("restaurants")]
    public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantRequest request)
    {
        var result = await _service.CreateRestaurantAsync(UserId, request);
        return Ok(result);
    }

    // PUT /api/organizations/restaurants/{id}
    // Нельзя поставить IsActive = true без Lat/Lng — вернёт 400
    [Authorize(Roles = "OrganizationOwner")]
    [HttpPut("restaurants/{id}")]
    public async Task<IActionResult> UpdateRestaurant(Guid id, [FromBody] UpdateRestaurantRequest request)
    {
        var result = await _service.UpdateRestaurantAsync(UserId, id, request);
        return Ok(result);
    }

    // DELETE /api/organizations/restaurants/{id}
    [Authorize(Roles = "OrganizationOwner")]
    [HttpDelete("restaurants/{id}")]
    public async Task<IActionResult> DeleteRestaurant(Guid id)
    {
        await _service.DeleteRestaurantAsync(UserId, id);
        return NoContent();
    }
}
