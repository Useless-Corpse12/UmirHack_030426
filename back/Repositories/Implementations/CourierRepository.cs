using DeliveryAggregator.Data;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryAggregator.Repositories.Implementations;

public class CourierRepository : ICourierRepository
{
    private readonly AppDbContext _db;
    public CourierRepository(AppDbContext db) => _db = db;

    public async Task<Courier?> GetByIdAsync(Guid id) =>
        await _db.Couriers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Courier?> GetByUserIdAsync(Guid userId) =>
        await _db.Couriers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);

    // Возвращает всех курьеров (для модератора)
    public async Task<List<Courier>> GetAllOnShiftAsync() =>
        await _db.Couriers
            .Include(c => c.User)
            .ToListAsync();

    public async Task AddAsync(Courier courier)
    {
        _db.Couriers.Add(courier);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Courier courier)
    {
        _db.Couriers.Update(courier);
        await _db.SaveChangesAsync();
    }
}
