namespace DeliveryAggregator.DTOs;

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,
    string Role,
    Guid UserId,
    string DisplayName
);

public record RegisterCustomerRequest(
    string Email,
    string Password,
    string DisplayName,
    string? ContactInfo
);

public record ChangePasswordRequest(
    string OldPassword,
    string NewPassword
);
