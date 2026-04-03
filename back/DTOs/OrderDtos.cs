using DeliveryAggregator.Enums;

namespace DeliveryAggregator.DTOs;

public record CreateOrderRequest(
    Guid RestaurantId,
    string DeliveryAddress,
    double? DeliveryLat,
    double? DeliveryLng,
    List<OrderItemRequest> Items
);

public record OrderItemRequest(
    Guid MenuItemId,
    int Quantity
);

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    Guid? CourierId,
    Guid RestaurantId,
    string RestaurantName,
    string OrgName,
    string Status,
    string DeliveryAddress,
    double? DeliveryLat,
    double? DeliveryLng,
    decimal TotalPrice,
    DateTime CreatedAt,
    DateTime? AcceptedAt,
    DateTime? DeliveredAt,
    List<OrderItemResponse> Items
);

public record OrderItemResponse(
    Guid Id,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);

// Для курьера ДО принятия: только цена, расстояние, откуда забрать
public record CourierOrderPreviewResponse(
    Guid Id,
    string RestaurantName,
    string RestaurantAddress,
    double? RestaurantLat,
    double? RestaurantLng,
    decimal TotalPrice
);

// Для курьера ПОСЛЕ принятия: полные детали
public record CourierOrderDetailsResponse(
    Guid Id,
    string RestaurantName,
    string RestaurantAddress,
    string DeliveryAddress,
    double? DeliveryLat,
    double? DeliveryLng,
    string CustomerContact,
    decimal TotalPrice,
    string Status,
    List<OrderItemResponse> Items
);
