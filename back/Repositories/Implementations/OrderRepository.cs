using DeliveryAggregator.Data;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Enums;
using DeliveryAggregator.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryAggregator.Repositories.Implementations;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;

    public async Task<Order?> GetByIdAsync(Guid id) =>
        await _db.Orders.FindAsync(id);

    public async Task<Order?> GetByIdWithItemsAsync(Guid id) =>
        await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Restaurant)
            .Include(o => o.Organization)
            .Include(o => o.Courier).ThenInclude(c => c!.User)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<List<Order>> GetByCustomerIdAsync(Guid customerId) =>
        await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Restaurant)
            .Include(o => o.Organization)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<List<Order>> GetByOrgIdAsync(Guid orgId) =>
        await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Restaurant)
            .Include(o => o.Organization)
            .Where(o => o.OrgId == orgId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<List<Order>> GetAvailableForCouriersAsync() =>
        await _db.Orders
            .Include(o => o.Restaurant)
            .Where(o => o.Status == OrderStatus.ReadyForPickup && o.CourierId == null)
            .ToListAsync();

    public async Task<List<Order>> GetByCourierIdAsync(Guid courierId) =>
        await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Restaurant)
            .Include(o => o.Organization)
            .Where(o => o.CourierId == courierId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(Order order)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
    }

    // Атомарное принятие заказа — защита от гонки двух курьеров одновременно
    public async Task<bool> TryAcceptOrderAsync(Guid orderId, Guid courierId)
    {
        var updated = await _db.Orders
            .Where(o => o.Id == orderId
                     && o.Status == OrderStatus.ReadyForPickup
                     && o.CourierId == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(o => o.CourierId, courierId)
                .SetProperty(o => o.Status, OrderStatus.InDelivery)
                .SetProperty(o => o.AcceptedAt, DateTime.UtcNow));

        return updated > 0;
    }
}
