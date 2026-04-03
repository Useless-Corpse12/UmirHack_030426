using DeliveryAggregator.Entities;
using DeliveryAggregator.Enums;

namespace DeliveryAggregator.Repositories.Mock;

// Единое in-memory хранилище — живёт пока работает приложение
public static class MockData
{
    public static List<User> Users { get; } = new()
    {
        new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Email = "moderator@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
            DisplayName = "Модератор",
            ContactInfo = "moderator@test.com",
            Role = UserRole.Moderator
        },
        new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Email = "owner@dodopizza.ru",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Owner123"),
            DisplayName = "Додо Пицца",
            ContactInfo = "+7 999 111 22 33",
            Role = UserRole.OrganizationOwner
        },
        new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Email = "courier@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Courier123"),
            DisplayName = "Иван Петров",
            ContactInfo = "+7 999 444 55 66",
            Role = UserRole.Courier
        },
        new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Email = "customer@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123"),
            DisplayName = "Алексей Смирнов",
            ContactInfo = "+7 999 777 88 99",
            Role = UserRole.Customer
        }
    };

    public static List<Organization> Organizations { get; } = new()
    {
        new Organization
        {
            Id = Guid.Parse("00000000-0000-0000-0001-000000000001"),
            OwnerId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Name = "Додо Пицца",
            IsBlocked = false,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        }
    };

    public static List<Restaurant> Restaurants { get; } = new()
    {
        new Restaurant
        {
            Id = Guid.Parse("00000000-0000-0000-0002-000000000001"),
            OrgId = Guid.Parse("00000000-0000-0000-0001-000000000001"),
            Name = "Додо на Ленина",
            Address = "ул. Ленина, 1",
            Lat = 55.7558, Lng = 37.6173,
            DeliveryRadius = 5,
            IsActive = true
        },
        new Restaurant
        {
            Id = Guid.Parse("00000000-0000-0000-0002-000000000002"),
            OrgId = Guid.Parse("00000000-0000-0000-0001-000000000001"),
            Name = "Додо ТЦ Мега",
            Address = "ТЦ Мега, ул. Пушкина, 10",
            Lat = 55.7600, Lng = 37.6200,
            DeliveryRadius = 3,
            IsActive = true
        }
    };

    public static List<MenuItem> MenuItems { get; } = new()
    {
        new MenuItem
        {
            Id = Guid.Parse("00000000-0000-0000-0003-000000000001"),
            OrgId = Guid.Parse("00000000-0000-0000-0001-000000000001"),
            Category = "Пицца",
            Name = "Пепперони",
            Description = "Томатный соус, моцарелла, пепперони",
            Price = 499,
            IsAvailable = true
        },
        new MenuItem
        {
            Id = Guid.Parse("00000000-0000-0000-0003-000000000002"),
            OrgId = Guid.Parse("00000000-0000-0000-0001-000000000001"),
            Category = "Пицца",
            Name = "Маргарита",
            Description = "Томатный соус, моцарелла, базилик",
            Price = 399,
            IsAvailable = true
        },
        new MenuItem
        {
            Id = Guid.Parse("00000000-0000-0000-0003-000000000003"),
            OrgId = Guid.Parse("00000000-0000-0000-0001-000000000001"),
            Category = "Напитки",
            Name = "Кола 0.5л",
            Description = "Coca-Cola",
            Price = 99,
            IsAvailable = true
        },
        new MenuItem
        {
            Id = Guid.Parse("00000000-0000-0000-0003-000000000004"),
            OrgId = Guid.Parse("00000000-0000-0000-0001-000000000001"),
            Category = "Закуски",
            Name = "Картошка фри",
            Description = "Хрустящая картошка фри",
            Price = 149,
            IsAvailable = true
        }
    };

    public static List<Courier> Couriers { get; } = new()
    {
        new Courier
        {
            Id = Guid.Parse("00000000-0000-0000-0004-000000000001"),
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            WorkZone = "Центральный район",
            IsOnShift = false,
            IsBlocked = false,
            CurrentOrderId = null
        }
    };

    public static List<Order> Orders { get; } = new()
    {
        new Order
        {
            Id = Guid.Parse("00000000-0000-0000-0005-000000000001"),
            CustomerId = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            CourierId = null,
            RestaurantId = Guid.Parse("00000000-0000-0000-0002-000000000001"),
            OrgId = Guid.Parse("00000000-0000-0000-0001-000000000001"),
            Status = OrderStatus.Pending,
            DeliveryAddress = "ул. Гагарина, 5, кв. 12",
            DeliveryLat = 55.7520, DeliveryLng = 37.6100,
            TotalPrice = 747,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = Guid.Parse("00000000-0000-0000-0005-000000000001"),
                    MenuItemId = Guid.Parse("00000000-0000-0000-0003-000000000001"),
                    Name = "Пепперони", Quantity = 1, UnitPrice = 499
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = Guid.Parse("00000000-0000-0000-0005-000000000001"),
                    MenuItemId = Guid.Parse("00000000-0000-0000-0003-000000000003"),
                    Name = "Кола 0.5л", Quantity = 1, UnitPrice = 99
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = Guid.Parse("00000000-0000-0000-0005-000000000001"),
                    MenuItemId = Guid.Parse("00000000-0000-0000-0003-000000000004"),
                    Name = "Картошка фри", Quantity = 1, UnitPrice = 149
                }
            }
        }
    };

    public static List<RegistrationApplication> Applications { get; } = new()
    {
        new RegistrationApplication
        {
            Id = Guid.Parse("00000000-0000-0000-0006-000000000001"),
            Email = "newcourier@test.com",
            DisplayName = "Сергей Новиков",
            Role = ApplicationRole.Courier,
            Status = ApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        }
    };

    // Навешиваем навигационные свойства один раз при старте
    static MockData()
    {
        var org = Organizations[0];
        var owner = Users[1];
        var courier = Couriers[0];
        var courierUser = Users[2];

        org.Owner = owner;
        org.Restaurants = Restaurants;
        org.MenuItems = MenuItems;

        foreach (var r in Restaurants) r.Organization = org;
        foreach (var m in MenuItems) m.Organization = org;

        courier.User = courierUser;

        var order = Orders[0];
        order.Restaurant = Restaurants[0];
        order.Organization = org;
        order.Customer = Users[3];
    }
}
