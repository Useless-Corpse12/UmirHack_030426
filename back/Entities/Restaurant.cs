namespace DeliveryAggregator.Entities;

public class Restaurant
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public double? DeliveryRadius { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
