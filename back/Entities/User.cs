using DeliveryAggregator.Enums;

namespace DeliveryAggregator.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? ContactInfo { get; set; }          // хранится зашифрованным (AES)
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;       // soft delete
    public DateTime? DeletedAt { get; set; }
    public bool IsEmailConfirmed { get; set; } = false;
    public List<string> Strikes { get; set; } = new(); // jsonb в PostgreSQL
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Organization? Organization { get; set; }
    public Courier? Courier { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<EmailConfirmationToken> EmailTokens { get; set; } = new List<EmailConfirmationToken>();
}
