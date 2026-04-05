using DeliveryAggregator.Enums;

namespace DeliveryAggregator.DTOs;

// Заявка от курьера или владельца организации — только ФИО/название + email + роль
public record CreateApplicationRequest(
    string Email,
    string DisplayName,  // ФИО для курьера / название организации
    ApplicationRole Role
);

public record ApplicationResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    string Status,
    string? ModeratorNote,
    DateTime CreatedAt,
    DateTime? ReviewedAt
);
