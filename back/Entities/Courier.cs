namespace DeliveryAggregator.Entities;

public class Courier
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? WorkZone { get; set; }
    public bool IsOnShift { get; set; } = false;
    public bool IsBlocked { get; set; } = false;
    public List<string> Strikes { get; set; } = new(); // причины страйков, jsonb
    public Guid? CurrentOrderId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Order? CurrentOrder { get; set; }
    public ICollection<Order> DeliveredOrders { get; set; } = new List<Order>();
}
