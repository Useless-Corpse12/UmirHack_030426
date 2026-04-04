using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DeliveryAggregator.Data;

// Используется только командами dotnet ef (migrations, update)
// Не влияет на работу приложения
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=delivery_aggregator;Username=postgres;Password=postgres");
        return new AppDbContext(optionsBuilder.Options);
    }
}
