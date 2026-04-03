namespace DeliveryAggregator.Entities;

public class MenuItem
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public string? Category { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
