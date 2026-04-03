using DeliveryAggregator.Data;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryAggregator.Repositories.Implementations;

public class MenuRepository : IMenuRepository
{
    private readonly AppDbContext _db;
    public MenuRepository(AppDbContext db) => _db = db;

    public async Task<MenuItem?> GetByIdAsync(Guid id) =>
        await _db.MenuItems.FindAsync(id);

    public async Task<List<MenuItem>> GetByOrgIdAsync(Guid orgId) =>
        await _db.MenuItems
            .Where(m => m.OrgId == orgId)
            .ToListAsync();

    public async Task AddAsync(MenuItem item)
    {
        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(MenuItem item)
    {
        _db.MenuItems.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item != null)
        {
            _db.MenuItems.Remove(item);
            await _db.SaveChangesAsync();
        }
    }
}
