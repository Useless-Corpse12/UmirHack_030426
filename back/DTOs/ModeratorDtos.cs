using DeliveryAggregator.Enums;

namespace DeliveryAggregator.DTOs;

public record CreateUserRequest(
    Guid ApplicationId,
    string Email,
    string Password,
    string DisplayName,
    string? ContactInfo,
    UserRole Role,
    string? WorkZone
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
    bool IsDeleted,
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
    List<string> Strikes,
    Guid? CurrentOrderId
);

public record ModeratorOrgResponse(
    Guid Id,
    string Name,
    string OwnerEmail,
    string OwnerName,
    bool IsBlocked,
    List<string> Strikes,
    int RestaurantCount,
    DateTime CreatedAt
);

public record BlockRequest(bool IsBlocked);

// Добавить страйк курьеру или организации
public record AddStrikeRequest(string Reason);

// Уволить/удалить курьера (soft delete)
public record FireCourierRequest(string Reason);
