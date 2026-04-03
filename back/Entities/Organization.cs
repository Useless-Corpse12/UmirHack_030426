namespace DeliveryAggregator.Entities;

public class Organization
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = null!;
    public bool IsBlocked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Owner { get; set; } = null!;
    public ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
