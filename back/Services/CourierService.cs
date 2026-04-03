using DeliveryAggregator.DTOs;
using DeliveryAggregator.Enums;
using DeliveryAggregator.Hubs;
using DeliveryAggregator.Repositories.Interfaces;
using DeliveryAggregator.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DeliveryAggregator.Services;

public class CourierService : ICourierService
{
    private readonly ICourierRepository _couriers;
    private readonly IOrderRepository _orders;
    private readonly IUserRepository _users;
    private readonly IHubContext<OrderHub> _hub;

    public CourierService(
        ICourierRepository couriers,
        IOrderRepository orders,
        IUserRepository users,
        IHubContext<OrderHub> hub)
    {
        _couriers = couriers;
        _orders = orders;
        _users = users;
        _hub = hub;
    }

    public async Task StartShiftAsync(Guid userId)
    {
        var courier = await _couriers.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Профиль курьера не найден");

        if (courier.IsBlocked)
            throw new InvalidOperationException("Аккаунт заблокирован");

        courier.IsOnShift = true;
        await _couriers.UpdateAsync(courier);
    }

    public async Task EndShiftAsync(Guid userId)
    {
        var courier = await _couriers.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Профиль курьера не найден");

        if (courier.CurrentOrderId != null)
            throw new InvalidOperationException("Нельзя завершить смену с активным заказом");

        courier.IsOnShift = false;
        await _couriers.UpdateAsync(courier);
    }

    public async Task<List<CourierOrderPreviewResponse>> GetAvailableOrdersAsync(Guid userId)
    {
        var courier = await _couriers.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Профиль курьера не найден");

        if (!courier.IsOnShift)
            throw new InvalidOperationException("Начните смену чтобы видеть заказы");

        var orders = await _orders.GetAvailableForCouriersAsync();

        return orders.Select(o => new CourierOrderPreviewResponse(
            o.Id,
            o.Restaurant?.Name ?? "",
            o.Restaurant?.Address ?? "",
            o.Restaurant?.Lat,
            o.Restaurant?.Lng,
            o.TotalPrice
        )).ToList();
    }

    public async Task<CourierOrderDetailsResponse> AcceptOrderAsync(Guid userId, Guid orderId)
    {
        var courier = await _couriers.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Профиль курьера не найден");

        if (courier.IsBlocked)
            throw new InvalidOperationException("Аккаунт заблокирован");

        if (!courier.IsOnShift)
            throw new InvalidOperationException("Сначала начните смену");

        if (courier.CurrentOrderId != null)
            throw new InvalidOperationException("У вас уже есть активный заказ");

        // Атомарное принятие — защита от гонки
        var accepted = await _orders.TryAcceptOrderAsync(orderId, courier.Id);
        if (!accepted)
            throw new InvalidOperationException("Заказ уже принят другим курьером");

        courier.CurrentOrderId = orderId;
        await _couriers.UpdateAsync(courier);

        // SignalR: сообщить всем курьерам что заказ пропал
        await _hub.Clients.Group("courier-feed")
            .SendAsync("OrderTaken", new { orderId });

        // SignalR: сообщить покупателю об обновлении статуса
        await _hub.Clients.Group($"order-{orderId}")
            .SendAsync("OrderStatusChanged", new { orderId, status = OrderStatus.InDelivery.ToString() });

        var order = await _orders.GetByIdWithItemsAsync(orderId)
            ?? throw new KeyNotFoundException("Заказ не найден");

        var customer = await _users.GetByIdAsync(order.CustomerId);

        return new CourierOrderDetailsResponse(
            order.Id,
            order.Restaurant?.Name ?? "",
            order.Restaurant?.Address ?? "",
            order.DeliveryAddress,
            order.DeliveryLat,
            order.DeliveryLng,
            customer?.ContactInfo ?? customer?.Email ?? "",
            order.TotalPrice,
            order.Status.ToString(),
            order.Items.Select(i => new OrderItemResponse(
                i.Id, i.Name, i.Quantity, i.UnitPrice, i.UnitPrice * i.Quantity
            )).ToList()
        );
    }

    public async Task<OrderResponse> CompleteOrderAsync(Guid userId, Guid orderId)
    {
        var courier = await _couriers.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Профиль курьера не найден");

        if (courier.CurrentOrderId != orderId)
            throw new InvalidOperationException("Этот заказ не ваш");

        var order = await _orders.GetByIdWithItemsAsync(orderId)
            ?? throw new KeyNotFoundException("Заказ не найден");

        order.Status = OrderStatus.Delivered;
        order.DeliveredAt = DateTime.UtcNow;
        await _orders.UpdateAsync(order);

        courier.CurrentOrderId = null;
        await _couriers.UpdateAsync(courier);

        // SignalR: уведомить покупателя
        await _hub.Clients.Group($"order-{orderId}")
            .SendAsync("OrderStatusChanged", new { orderId, status = OrderStatus.Delivered.ToString() });

        return OrderService.MapOrder(order);
    }

    public async Task<List<OrderResponse>> GetMyDeliveredOrdersAsync(Guid userId)
    {
        var courier = await _couriers.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Профиль курьера не найден");

        var orders = await _orders.GetByCourierIdAsync(courier.Id);
        return orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .Select(OrderService.MapOrder)
            .ToList();
    }

    public async Task<CourierOrderDetailsResponse?> GetCurrentOrderAsync(Guid userId)
    {
        var courier = await _couriers.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Профиль курьера не найден");

        if (courier.CurrentOrderId == null) return null;

        var order = await _orders.GetByIdWithItemsAsync(courier.CurrentOrderId.Value);
        if (order == null) return null;

        var customer = await _users.GetByIdAsync(order.CustomerId);

        return new CourierOrderDetailsResponse(
            order.Id,
            order.Restaurant?.Name ?? "",
            order.Restaurant?.Address ?? "",
            order.DeliveryAddress,
            order.DeliveryLat,
            order.DeliveryLng,
            customer?.ContactInfo ?? customer?.Email ?? "",
            order.TotalPrice,
            order.Status.ToString(),
            order.Items.Select(i => new OrderItemResponse(
                i.Id, i.Name, i.Quantity, i.UnitPrice, i.UnitPrice * i.Quantity
            )).ToList()
        );
    }
}
