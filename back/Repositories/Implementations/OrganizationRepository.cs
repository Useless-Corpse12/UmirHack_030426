using DeliveryAggregator.Data;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryAggregator.Repositories.Implementations;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly AppDbContext _db;
    public OrganizationRepository(AppDbContext db) => _db = db;

    public async Task<Organization?> GetByIdAsync(Guid id) =>
        await _db.Organizations
            .Include(o => o.Owner)
            .Include(o => o.Restaurants)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Organization?> GetByOwnerIdAsync(Guid ownerId) =>
        await _db.Organizations
            .Include(o => o.Owner)
            .Include(o => o.Restaurants)
            .FirstOrDefaultAsync(o => o.OwnerId == ownerId);

    public async Task<List<Organization>> GetAllActiveAsync() =>
        await _db.Organizations
            .Include(o => o.Owner)
            .Include(o => o.Restaurants)
            .Where(o => !o.IsBlocked)
            .ToListAsync();

    public async Task AddAsync(Organization organization)
    {
        _db.Organizations.Add(organization);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Organization organization)
    {
        _db.Organizations.Update(organization);
        await _db.SaveChangesAsync();
    }
}
