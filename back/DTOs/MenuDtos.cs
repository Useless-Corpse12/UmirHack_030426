namespace DeliveryAggregator.DTOs;

public record CreateMenuItemRequest(
    string Name,
    string? Category,
    string? Description,
    decimal Price,
    string? PhotoUrl
);

public record UpdateMenuItemRequest(
    string Name,
    string? Category,
    string? Description,
    decimal Price,
    string? PhotoUrl,
    bool IsAvailable
);

public record MenuItemResponse(
    Guid Id,
    Guid OrgId,
    string? Category,
    string Name,
    string? Description,
    decimal Price,
    string? PhotoUrl,
    bool IsAvailable
);

// Меню организации, сгруппированное по категориям
public record MenuResponse(
    Guid OrgId,
    string OrgName,
    List<MenuCategoryResponse> Categories
);

public record MenuCategoryResponse(
    string? Category,
    List<MenuItemResponse> Items
);
