namespace DeliveryAggregator.DTOs;

public record OrganizationResponse(
    Guid Id,
    string Name,
    bool IsBlocked,
    DateTime CreatedAt,
    List<RestaurantResponse> Restaurants
);

public record CreateRestaurantRequest(
    string Name,
    string Address,
    double? Lat,
    double? Lng,
    double? DeliveryRadius
);

public record UpdateRestaurantRequest(
    string Name,
    string Address,
    double? Lat,
    double? Lng,
    double? DeliveryRadius,
    bool IsActive
);

public record RestaurantResponse(
    Guid Id,
    Guid OrgId,
    string OrgName,
    string Name,
    string Address,
    double? Lat,
    double? Lng,
    double? DeliveryRadius,
    bool IsActive
);

// Для покупателя — список организаций чтобы выбрать откуда заказывать
public record OrganizationListItemResponse(
    Guid Id,
    string Name,
    int RestaurantCount
);
