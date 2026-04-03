using DeliveryAggregator.Enums;

namespace DeliveryAggregator.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? ContactInfo { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Organization? Organization { get; set; }
    public Courier? Courier { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
