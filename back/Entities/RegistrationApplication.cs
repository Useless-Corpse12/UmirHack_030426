using DeliveryAggregator.Enums;

namespace DeliveryAggregator.Entities;

public class RegistrationApplication
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!; // ФИО для курьера / название орги
    public ApplicationRole Role { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public string? ModeratorNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}
