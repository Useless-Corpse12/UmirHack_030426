using DeliveryAggregator.Data;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryAggregator.Repositories.Implementations;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly AppDbContext _db;
    public RestaurantRepository(AppDbContext db) => _db = db;

    public async Task<Restaurant?> GetByIdAsync(Guid id) =>
        await _db.Restaurants
            .Include(r => r.Organization)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<Restaurant>> GetByOrgIdAsync(Guid orgId) =>
        await _db.Restaurants
            .Where(r => r.OrgId == orgId)
            .ToListAsync();

    public async Task<List<Restaurant>> GetAllActiveAsync() =>
        await _db.Restaurants
            .Include(r => r.Organization)
            .Where(r => r.IsActive && !r.Organization.IsBlocked)
            .ToListAsync();

    public async Task AddAsync(Restaurant restaurant)
    {
        _db.Restaurants.Add(restaurant);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Restaurant restaurant)
    {
        _db.Restaurants.Update(restaurant);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var restaurant = await _db.Restaurants.FindAsync(id);
        if (restaurant != null)
        {
            _db.Restaurants.Remove(restaurant);
            await _db.SaveChangesAsync();
        }
    }
}
