namespace DeliveryAggregator.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = null!;       // снапшот названия
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }           // снапшот цены

    // Navigation
    public Order Order { get; set; } = null!;
    public MenuItem MenuItem { get; set; } = null!;
}
