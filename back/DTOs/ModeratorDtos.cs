using DeliveryAggregator.Enums;

namespace DeliveryAggregator.DTOs;

// Модератор создаёт пользователя вручную при одобрении заявки
public record CreateUserRequest(
    Guid ApplicationId,
    string Email,
    string Password,
    string DisplayName,
    string? ContactInfo,
    UserRole Role,
    string? WorkZone  // только для курьера
);

public record ReviewApplicationRequest(
    ApplicationStatus Status,
    string? ModeratorNote
);

public record ModeratorUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string? ContactInfo,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);

public record ModeratorCourierResponse(
    Guid Id,
    Guid UserId,
    string DisplayName,
    string Email,
    string? WorkZone,
    bool IsOnShift,
    bool IsBlocked,
    Guid? CurrentOrderId
);

public record ModeratorOrgResponse(
    Guid Id,
    string Name,
    string OwnerEmail,
    string OwnerName,
    bool IsBlocked,
    int RestaurantCount,
    DateTime CreatedAt
);

public record BlockRequest(bool IsBlocked);
