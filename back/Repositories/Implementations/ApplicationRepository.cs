using DeliveryAggregator.Data;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Enums;
using DeliveryAggregator.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryAggregator.Repositories.Implementations;

public class ApplicationRepository : IApplicationRepository
{
    private readonly AppDbContext _db;
    public ApplicationRepository(AppDbContext db) => _db = db;

    public async Task<RegistrationApplication?> GetByIdAsync(Guid id) =>
        await _db.Applications.FindAsync(id);

    public async Task<List<RegistrationApplication>> GetAllPendingAsync() =>
        await _db.Applications
            .Where(a => a.Status == ApplicationStatus.Pending)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

    public async Task<List<RegistrationApplication>> GetAllAsync() =>
        await _db.Applications
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(RegistrationApplication application)
    {
        _db.Applications.Add(application);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(RegistrationApplication application)
    {
        _db.Applications.Update(application);
        await _db.SaveChangesAsync();
    }
}
