using DeliveryAggregator.Enums;

namespace DeliveryAggregator.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? CourierId { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid OrgId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string DeliveryAddress { get; set; } = null!;
    public double? DeliveryLat { get; set; }
    public double? DeliveryLng { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // Navigation
    public User Customer { get; set; } = null!;
    public Courier? Courier { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
