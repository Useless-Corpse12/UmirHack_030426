using System.Security.Claims;
using DeliveryAggregator.DTOs;
using DeliveryAggregator.Enums;
using DeliveryAggregator.Hubs;
using DeliveryAggregator.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DeliveryAggregator.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;
    private readonly IHubContext<OrderHub> _hub;

    public OrdersController(IOrderService service, IHubContext<OrderHub> hub)
    {
        _service = service;
        _hub = hub;
    }

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string UserRole => User.FindFirst(ClaimTypes.Role)!.Value;

    // POST /api/orders
    // Покупатель создаёт заказ
    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var result = await _service.CreateOrderAsync(UserId, request);

        // SignalR: уведомить ресторан о новом заказе
        await _hub.Clients.Group($"org-{result.RestaurantId}")
            .SendAsync("NewOrder", new { orderId = result.Id });

        return Ok(result);
    }

    // GET /api/orders
    // Покупатель видит свои заказы, владелец — заказы своей орги
    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        if (UserRole == "Customer")
        {
            var orders = await _service.GetMyOrdersAsync(UserId);
            return Ok(orders);
        }
        if (UserRole == "OrganizationOwner")
        {
            var orders = await _service.GetOrgOrdersAsync(UserId);
            return Ok(orders);
        }
        return Forbid();
    }

    // GET /api/orders/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id, UserId, UserRole);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // PATCH /api/orders/{id}/status
    // Ресторан обновляет статус заказа
    [Authorize(Roles = "OrganizationOwner")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, out var status))
            return BadRequest("Неверный статус");

        var result = await _service.UpdateStatusAsync(id, UserId, UserRole, status);

        // SignalR: уведомить покупателя
        await _hub.Clients.Group($"order-{id}")
            .SendAsync("OrderStatusChanged", new { orderId = id, status = result.Status });

        // SignalR: если заказ готов к выдаче — уведомить курьеров
        if (result.Status == OrderStatus.ReadyForPickup.ToString())
        {
            await _hub.Clients.Group("courier-feed")
                .SendAsync("OrderAvailable", new
                {
                    orderId = result.Id,
                    restaurantName = result.RestaurantName,
                    price = result.TotalPrice
                });
        }

        return Ok(result);
    }
}

public record UpdateStatusRequest(string Status);
