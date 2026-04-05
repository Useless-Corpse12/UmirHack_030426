# ТЗ для Бек 2 — База данных и репозитории

## Твоя зона ответственности
- AppDbContext (EF Core + PostgreSQL)
- Конфигурации сущностей (Fluent API)
- Миграции
- Реализация всех репозиториев из `Repositories/Interfaces/IRepositories.cs`

Бек 1 пишет сервисы и контроллеры. Он вызывает твои репозитории через интерфейсы — не трогает DbContext напрямую.

---

## Пакеты (уже в .csproj)
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Npgsql.EntityFrameworkCore.PostgreSQL`

---

## Шаг 1 — AppDbContext

Создать файл `Data/AppDbContext.cs`:

```csharp
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(e => {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>();
        });

        // RegistrationApplication
        modelBuilder.Entity<RegistrationApplication>(e => {
            e.HasKey(x => x.Id);
            e.Property(x => x.Role).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
        });

        // Organization
        modelBuilder.Entity<Organization>(e => {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Owner)
             .WithOne(x => x.Organization)
             .HasForeignKey<Organization>(x => x.OwnerId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Restaurants)
             .WithOne(x => x.Organization)
             .HasForeignKey(x => x.OrgId);
            e.HasMany(x => x.MenuItems)
             .WithOne(x => x.Organization)
             .HasForeignKey(x => x.OrgId);
        });

        // Courier — осторожно с циклической зависимостью Courier <-> Order
        modelBuilder.Entity<Courier>(e => {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User)
             .WithOne(x => x.Courier)
             .HasForeignKey<Courier>(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);
            // current_order — nullable FK, без cascade
            e.HasOne(x => x.CurrentOrder)
             .WithMany()
             .HasForeignKey(x => x.CurrentOrderId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Order
        modelBuilder.Entity<Order>(e => {
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

        // OrderItem
        modelBuilder.Entity<OrderItem>(e => {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.MenuItem)
             .WithMany(x => x.OrderItems)
             .HasForeignKey(x => x.MenuItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Индексы
        modelBuilder.Entity<Order>()
            .HasIndex(x => x.Status);
        modelBuilder.Entity<Order>()
            .HasIndex(x => x.CourierId);
        modelBuilder.Entity<Order>()
            .HasIndex(x => x.CustomerId);
        modelBuilder.Entity<Courier>()
            .HasIndex(x => x.IsOnShift);
        modelBuilder.Entity<MenuItem>()
            .HasIndex(x => x.OrgId);
    }
}
```

---

## Шаг 2 — Миграция

После создания AppDbContext:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## Шаг 3 — Реализация репозиториев

Создать папку `Repositories/Implementations/`. Для каждого репозитория — свой файл.

### UserRepository.cs
```csharp
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(Guid id) =>
        await _db.Users.FindAsync(id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<List<User>> GetAllAsync() =>
        await _db.Users.ToListAsync();

    public async Task AddAsync(User user) {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user) {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }
}
```

### ApplicationRepository.cs
```csharp
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
        await _db.Applications.OrderByDescending(a => a.CreatedAt).ToListAsync();

    public async Task AddAsync(RegistrationApplication app) {
        _db.Applications.Add(app);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(RegistrationApplication app) {
        _db.Applications.Update(app);
        await _db.SaveChangesAsync();
    }
}
```

### OrganizationRepository.cs
```csharp
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
            .Include(o => o.Restaurants)
            .FirstOrDefaultAsync(o => o.OwnerId == ownerId);

    public async Task<List<Organization>> GetAllActiveAsync() =>
        await _db.Organizations
            .Include(o => o.Owner)
            .Include(o => o.Restaurants)
            .Where(o => !o.IsBlocked)
            .ToListAsync();

    public async Task AddAsync(Organization org) {
        _db.Organizations.Add(org);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Organization org) {
        _db.Organizations.Update(org);
        await _db.SaveChangesAsync();
    }
}
```

### RestaurantRepository.cs
```csharp
public class RestaurantRepository : IRestaurantRepository
{
    private readonly AppDbContext _db;
    public RestaurantRepository(AppDbContext db) => _db = db;

    public async Task<Restaurant?> GetByIdAsync(Guid id) =>
        await _db.Restaurants.Include(r => r.Organization).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<Restaurant>> GetByOrgIdAsync(Guid orgId) =>
        await _db.Restaurants.Where(r => r.OrgId == orgId).ToListAsync();

    public async Task<List<Restaurant>> GetAllActiveAsync() =>
        await _db.Restaurants
            .Include(r => r.Organization)
            .Where(r => r.IsActive && !r.Organization.IsBlocked)
            .ToListAsync();

    public async Task AddAsync(Restaurant r) { _db.Restaurants.Add(r); await _db.SaveChangesAsync(); }
    public async Task UpdateAsync(Restaurant r) { _db.Restaurants.Update(r); await _db.SaveChangesAsync(); }
    public async Task DeleteAsync(Guid id) {
        var r = await _db.Restaurants.FindAsync(id);
        if (r != null) { _db.Restaurants.Remove(r); await _db.SaveChangesAsync(); }
    }
}
```

### MenuRepository.cs
```csharp
public class MenuRepository : IMenuRepository
{
    private readonly AppDbContext _db;
    public MenuRepository(AppDbContext db) => _db = db;

    public async Task<MenuItem?> GetByIdAsync(Guid id) => await _db.MenuItems.FindAsync(id);

    public async Task<List<MenuItem>> GetByOrgIdAsync(Guid orgId) =>
        await _db.MenuItems.Where(m => m.OrgId == orgId).ToListAsync();

    public async Task AddAsync(MenuItem item) { _db.MenuItems.Add(item); await _db.SaveChangesAsync(); }
    public async Task UpdateAsync(MenuItem item) { _db.MenuItems.Update(item); await _db.SaveChangesAsync(); }
    public async Task DeleteAsync(Guid id) {
        var item = await _db.MenuItems.FindAsync(id);
        if (item != null) { _db.MenuItems.Remove(item); await _db.SaveChangesAsync(); }
    }
}
```

### CourierRepository.cs
```csharp
public class CourierRepository : ICourierRepository
{
    private readonly AppDbContext _db;
    public CourierRepository(AppDbContext db) => _db = db;

    public async Task<Courier?> GetByIdAsync(Guid id) =>
        await _db.Couriers.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Courier?> GetByUserIdAsync(Guid userId) =>
        await _db.Couriers.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == userId);

    // Возвращает ВСЕХ курьеров (не только на смене) — для модератора
    public async Task<List<Courier>> GetAllOnShiftAsync() =>
        await _db.Couriers.Include(c => c.User).ToListAsync();

    public async Task AddAsync(Courier c) { _db.Couriers.Add(c); await _db.SaveChangesAsync(); }
    public async Task UpdateAsync(Courier c) { _db.Couriers.Update(c); await _db.SaveChangesAsync(); }
}
```

### OrderRepository.cs — ВАЖНО: TryAcceptOrderAsync атомарный!
```csharp
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;

    public async Task<Order?> GetByIdAsync(Guid id) => await _db.Orders.FindAsync(id);

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

    public async Task AddAsync(Order order) { _db.Orders.Add(order); await _db.SaveChangesAsync(); }
    public async Task UpdateAsync(Order order) { _db.Orders.Update(order); await _db.SaveChangesAsync(); }

    // Атомарное принятие заказа — защита от гонки двух курьеров
    public async Task<bool> TryAcceptOrderAsync(Guid orderId, Guid courierId)
    {
        // ExecuteUpdateAsync делает UPDATE...WHERE атомарно на уровне БД
        var updated = await _db.Orders
            .Where(o => o.Id == orderId
                     && o.Status == OrderStatus.ReadyForPickup
                     && o.CourierId == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(o => o.CourierId, courierId)
                .SetProperty(o => o.Status, OrderStatus.InDelivery)
                .SetProperty(o => o.AcceptedAt, DateTime.UtcNow));

        return updated > 0; // 0 = уже взят кем-то, 1 = успешно
    }
}
```

---

## Шаг 4 — Добавить в Program.cs (скажи Бек 1)

После того как создал все репозитории, попроси Бек 1 раскомментировать в `Program.cs`:

```csharp
// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Репозитории
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<ICourierRepository, CourierRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
```

---

## Шаг 5 — Seed данные (опционально, но полезно для тестов)

Добавить в `OnModelCreating` или отдельный метод `SeedData`:
- 1 модератор: `moderator@test.com` / `Admin123`
- 1 тестовый покупатель
- 1 тестовая организация с рестораном и меню

---

## Итог файлов которые ты создаёшь

```
Data/
  AppDbContext.cs
Repositories/
  Implementations/
    UserRepository.cs
    ApplicationRepository.cs
    OrganizationRepository.cs
    RestaurantRepository.cs
    MenuRepository.cs
    CourierRepository.cs
    OrderRepository.cs
```

Интерфейсы уже готовы в `Repositories/Interfaces/IRepositories.cs` — реализуй их точно.
