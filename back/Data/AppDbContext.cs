using DeliveryAggregator.Entities;
using DeliveryAggregator.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeliveryAggregator.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RegistrationApplication> Applications => Set<RegistrationApplication>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Courier> Couriers => Set<Courier>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<EmailConfirmationToken> EmailConfirmationTokens => Set<EmailConfirmationToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>();
            // Strikes как jsonb в PostgreSQL
            e.Property(x => x.Strikes)
             .HasColumnType("jsonb")
             .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
             );
        });

        modelBuilder.Entity<RegistrationApplication>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Role).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
        });

        modelBuilder.Entity<Organization>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Owner)
             .WithOne(x => x.Organization)
             .HasForeignKey<Organization>(x => x.OwnerId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Restaurants)
             .WithOne(x => x.Organization)
             .HasForeignKey(x => x.OrgId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.MenuItems)
             .WithOne(x => x.Organization)
             .HasForeignKey(x => x.OrgId)
             .OnDelete(DeleteBehavior.Cascade);
            // Strikes как jsonb
            e.Property(x => x.Strikes)
             .HasColumnType("jsonb")
             .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
             );
        });

        modelBuilder.Entity<Courier>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User)
             .WithOne(x => x.Courier)
             .HasForeignKey<Courier>(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CurrentOrder)
             .WithMany()
             .HasForeignKey(x => x.CurrentOrderId)
             .OnDelete(DeleteBehavior.SetNull);
            // Strikes как jsonb
            e.Property(x => x.Strikes)
             .HasColumnType("jsonb")
             .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
             );
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Customer)
             .WithMany(x => x.Orders)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Courier)
             .WithMany(x => x.DeliveredOrders)
             .HasForeignKey(x => x.CourierId)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Restaurant)
             .WithMany(x => x.Orders)
             .HasForeignKey(x => x.RestaurantId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Organization)
             .WithMany()
             .HasForeignKey(x => x.OrgId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Items)
             .WithOne(x => x.Order)
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.MenuItem)
             .WithMany(x => x.OrderItems)
             .HasForeignKey(x => x.MenuItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmailConfirmationToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Token).IsUnique();
            e.HasOne(x => x.User)
             .WithMany(x => x.EmailTokens)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Индексы
        modelBuilder.Entity<Order>().HasIndex(x => x.Status);
        modelBuilder.Entity<Order>().HasIndex(x => x.CourierId);
        modelBuilder.Entity<Order>().HasIndex(x => x.CustomerId);
        modelBuilder.Entity<Courier>().HasIndex(x => x.IsOnShift);
        modelBuilder.Entity<MenuItem>().HasIndex(x => x.OrgId);
        modelBuilder.Entity<User>().HasIndex(x => x.IsDeleted);
    }
}
