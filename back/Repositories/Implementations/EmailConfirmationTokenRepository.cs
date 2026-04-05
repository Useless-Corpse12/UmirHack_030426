using DeliveryAggregator.Data;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryAggregator.Repositories.Implementations;

public class EmailConfirmationTokenRepository : IEmailConfirmationTokenRepository
{
    private readonly AppDbContext _db;
    public EmailConfirmationTokenRepository(AppDbContext db) => _db = db;

    public async Task<EmailConfirmationToken?> GetByTokenAsync(string token) =>
        await _db.EmailConfirmationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);

    public async Task<EmailConfirmationToken?> GetActiveByUserIdAsync(Guid userId) =>
        await _db.EmailConfirmationTokens
            .FirstOrDefaultAsync(t => t.UserId == userId
                                   && !t.IsUsed
                                   && t.ExpiresAt > DateTime.UtcNow);

    public async Task AddAsync(EmailConfirmationToken token)
    {
        _db.EmailConfirmationTokens.Add(token);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(EmailConfirmationToken token)
    {
        _db.EmailConfirmationTokens.Update(token);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteExpiredAsync()
    {
        await _db.EmailConfirmationTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync();
    }
}
