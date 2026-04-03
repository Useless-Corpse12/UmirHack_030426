using System.Security.Claims;
using DeliveryAggregator.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAggregator.Controllers;

[ApiController]
[Route("api/courier")]
[Authorize(Roles = "Courier")]
public class CouriersController : ControllerBase
{
    private readonly ICourierService _service;

    public CouriersController(ICourierService service) => _service = service;

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // POST /api/courier/shift/start
    [HttpPost("shift/start")]
    public async Task<IActionResult> StartShift()
    {
        await _service.StartShiftAsync(UserId);
        return Ok(new { message = "Смена начата" });
    }

    // POST /api/courier/shift/end
    [HttpPost("shift/end")]
    public async Task<IActionResult> EndShift()
    {
        await _service.EndShiftAsync(UserId);
        return Ok(new { message = "Смена завершена" });
    }

    // GET /api/courier/orders/available
    // Список доступных заказов: только цена + откуда забрать (адрес клиента скрыт)
    [HttpGet("orders/available")]
    public async Task<IActionResult> GetAvailableOrders()
    {
        var result = await _service.GetAvailableOrdersAsync(UserId);
        return Ok(result);
    }

    // GET /api/courier/orders/current
    // Текущий активный заказ с полными деталями
    [HttpGet("orders/current")]
    public async Task<IActionResult> GetCurrentOrder()
    {
        var result = await _service.GetCurrentOrderAsync(UserId);
        if (result == null) return Ok(new { message = "Нет активного заказа" });
        return Ok(result);
    }

    // GET /api/courier/orders/history
    // История выполненных доставок
    [HttpGet("orders/history")]
    public async Task<IActionResult> GetHistory()
    {
        var result = await _service.GetMyDeliveredOrdersAsync(UserId);
        return Ok(result);
    }

    // POST /api/courier/orders/{id}/accept
    // Принять заказ — атомарно, защита от гонки через TryAcceptOrderAsync
    [HttpPost("orders/{id}/accept")]
    public async Task<IActionResult> AcceptOrder(Guid id)
    {
        var result = await _service.AcceptOrderAsync(UserId, id);
        return Ok(result);
    }

    // POST /api/courier/orders/{id}/complete
    // Завершить доставку
    [HttpPost("orders/{id}/complete")]
    public async Task<IActionResult> CompleteOrder(Guid id)
    {
        var result = await _service.CompleteOrderAsync(UserId, id);
        return Ok(result);
    }
}
