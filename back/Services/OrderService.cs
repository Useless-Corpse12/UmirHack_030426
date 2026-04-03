using DeliveryAggregator.DTOs;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Enums;
using DeliveryAggregator.Repositories.Interfaces;
using DeliveryAggregator.Services.Interfaces;

namespace DeliveryAggregator.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly IRestaurantRepository _restaurants;
    private readonly IMenuRepository _menu;
    private readonly IOrganizationRepository _orgs;

    public OrderService(
        IOrderRepository orders,
        IRestaurantRepository restaurants,
        IMenuRepository menu,
        IOrganizationRepository orgs)
    {
        _orders = orders;
        _restaurants = restaurants;
        _menu = menu;
        _orgs = orgs;
    }

    public async Task<OrderResponse> CreateOrderAsync(Guid customerId, CreateOrderRequest request)
    {
        var restaurant = await _restaurants.GetByIdAsync(request.RestaurantId)
            ?? throw new KeyNotFoundException("Ресторан не найден");

        if (!restaurant.IsActive)
            throw new InvalidOperationException("Ресторан не активен");

        var org = await _orgs.GetByIdAsync(restaurant.OrgId)
            ?? throw new KeyNotFoundException("Организация не найдена");

        if (org.IsBlocked)
            throw new InvalidOperationException("Организация заблокирована");

        var orderItems = new List<OrderItem>();
        decimal total = 0;

        foreach (var itemReq in request.Items)
        {
            var menuItem = await _menu.GetByIdAsync(itemReq.MenuItemId)
                ?? throw new KeyNotFoundException($"Позиция меню {itemReq.MenuItemId} не найдена");

            if (!menuItem.IsAvailable)
                throw new InvalidOperationException($"Позиция '{menuItem.Name}' недоступна");

            if (menuItem.OrgId != org.Id)
                throw new InvalidOperationException("Позиция меню не принадлежит этой организации");

            var oi = new OrderItem
            {
                Id = Guid.NewGuid(),
                MenuItemId = menuItem.Id,
                Name = menuItem.Name,       // снапшот
                UnitPrice = menuItem.Price, // снапшот
                Quantity = itemReq.Quantity
            };
            orderItems.Add(oi);
            total += oi.UnitPrice * oi.Quantity;
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            RestaurantId = restaurant.Id,
            OrgId = org.Id,
            DeliveryAddress = request.DeliveryAddress,
            DeliveryLat = request.DeliveryLat,
            DeliveryLng = request.DeliveryLng,
            TotalPrice = total,
            Status = OrderStatus.Pending,
            Items = orderItems
        };

        await _orders.AddAsync(order);

        order.Restaurant = restaurant;
        order.Organization = org;
        return MapOrder(order);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid orderId, Guid requesterId, string requesterRole)
    {
        var order = await _orders.GetByIdWithItemsAsync(orderId);
        if (order == null) return null;

        // Проверка доступа
        bool hasAccess = requesterRole switch
        {
            "Moderator" => true,
            "Customer" => order.CustomerId == requesterId,
            "Courier" => order.Courier?.UserId == requesterId,
            "OrganizationOwner" => order.Organization?.OwnerId == requesterId,
            _ => false
        };

        if (!hasAccess) throw new UnauthorizedAccessException("Нет доступа к заказу");
        return MapOrder(order);
    }

    public async Task<List<OrderResponse>> GetMyOrdersAsync(Guid customerId)
    {
        var orders = await _orders.GetByCustomerIdAsync(customerId);
        return orders.Select(MapOrder).ToList();
    }

    public async Task<List<OrderResponse>> GetOrgOrdersAsync(Guid ownerId)
    {
        var org = await _orgs.GetByOwnerIdAsync(ownerId)
            ?? throw new InvalidOperationException("Организация не найдена");

        var orders = await _orders.GetByOrgIdAsync(org.Id);
        return orders.Select(MapOrder).ToList();
    }

    public async Task<OrderResponse> UpdateStatusAsync(
        Guid orderId, Guid requesterId, string requesterRole, OrderStatus newStatus)
    {
        var order = await _orders.GetByIdWithItemsAsync(orderId)
            ?? throw new KeyNotFoundException("Заказ не найден");

        // Ресторан может: Pending→Confirmed, Confirmed→ReadyForPickup, любой→Cancelled
        // Курьер меняет статус через CourierService (InDelivery→Delivered)
        if (requesterRole == "OrganizationOwner")
        {
            if (order.Organization?.OwnerId != requesterId)
                throw new UnauthorizedAccessException("Нет доступа");

            var allowed = (order.Status, newStatus) switch
            {
                (OrderStatus.Pending, OrderStatus.Confirmed) => true,
                (OrderStatus.Confirmed, OrderStatus.ReadyForPickup) => true,
                (_, OrderStatus.Cancelled) => true,
                _ => false
            };
            if (!allowed)
                throw new InvalidOperationException($"Нельзя перевести из {order.Status} в {newStatus}");
        }

        order.Status = newStatus;
        await _orders.UpdateAsync(order);
        return MapOrder(order);
    }

    public static OrderResponse MapOrder(Order o) => new(
        o.Id, o.CustomerId, o.CourierId,
        o.RestaurantId,
        o.Restaurant?.Name ?? "",
        o.Organization?.Name ?? "",
        o.Status.ToString(),
        o.DeliveryAddress, o.DeliveryLat, o.DeliveryLng,
        o.TotalPrice, o.CreatedAt, o.AcceptedAt, o.DeliveredAt,
        o.Items.Select(i => new OrderItemResponse(
            i.Id, i.Name, i.Quantity, i.UnitPrice, i.UnitPrice * i.Quantity
        )).ToList()
    );
}
