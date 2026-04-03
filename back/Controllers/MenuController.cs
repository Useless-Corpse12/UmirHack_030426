using System.Security.Claims;
using DeliveryAggregator.DTOs;
using DeliveryAggregator.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAggregator.Controllers;

[ApiController]
[Route("api/menu")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _service;

    public MenuController(IMenuService service) => _service = service;

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // GET /api/menu/{orgId}
    // Публичное — покупатель смотрит меню организации сгруппированное по категориям
    [HttpGet("{orgId}")]
    public async Task<IActionResult> GetMenu(Guid orgId)
    {
        var result = await _service.GetMenuByOrgIdAsync(orgId);
        return Ok(result);
    }

    // POST /api/menu
    [Authorize(Roles = "OrganizationOwner")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuItemRequest request)
    {
        var result = await _service.CreateItemAsync(UserId, request);
        return Ok(result);
    }

    // PUT /api/menu/{id}
    [Authorize(Roles = "OrganizationOwner")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMenuItemRequest request)
    {
        var result = await _service.UpdateItemAsync(UserId, id, request);
        return Ok(result);
    }

    // DELETE /api/menu/{id}
    [Authorize(Roles = "OrganizationOwner")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteItemAsync(UserId, id);
        return NoContent();
    }
}
