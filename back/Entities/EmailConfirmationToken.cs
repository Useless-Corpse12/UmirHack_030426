namespace DeliveryAggregator.Entities;

public class EmailConfirmationToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;

    // Navigation
    public User User { get; set; } = null!;
}
