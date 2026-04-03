using DeliveryAggregator.Entities;
using DeliveryAggregator.Enums;
using DeliveryAggregator.Repositories.Interfaces;

namespace DeliveryAggregator.Repositories.Mock;

public class MockUserRepository : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id) =>
        Task.FromResult(MockData.Users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string email) =>
        Task.FromResult(MockData.Users.FirstOrDefault(u => u.Email == email));

    public Task<List<User>> GetAllAsync() =>
        Task.FromResult(MockData.Users.ToList());

    public Task AddAsync(User user)
    {
        MockData.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user)
    {
        var i = MockData.Users.FindIndex(u => u.Id == user.Id);
        if (i >= 0) MockData.Users[i] = user;
        return Task.CompletedTask;
    }
}

public class MockApplicationRepository : IApplicationRepository
{
    public Task<RegistrationApplication?> GetByIdAsync(Guid id) =>
        Task.FromResult(MockData.Applications.FirstOrDefault(a => a.Id == id));

    public Task<List<RegistrationApplication>> GetAllPendingAsync() =>
        Task.FromResult(MockData.Applications.Where(a => a.Status == ApplicationStatus.Pending).ToList());

    public Task<List<RegistrationApplication>> GetAllAsync() =>
        Task.FromResult(MockData.Applications.OrderByDescending(a => a.CreatedAt).ToList());

    public Task AddAsync(RegistrationApplication application)
    {
        MockData.Applications.Add(application);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(RegistrationApplication application)
    {
        var i = MockData.Applications.FindIndex(a => a.Id == application.Id);
        if (i >= 0) MockData.Applications[i] = application;
        return Task.CompletedTask;
    }
}

public class MockOrganizationRepository : IOrganizationRepository
{
    public Task<Organization?> GetByIdAsync(Guid id) =>
        Task.FromResult(MockData.Organizations.FirstOrDefault(o => o.Id == id));

    public Task<Organization?> GetByOwnerIdAsync(Guid ownerId) =>
        Task.FromResult(MockData.Organizations.FirstOrDefault(o => o.OwnerId == ownerId));

    public Task<List<Organization>> GetAllActiveAsync() =>
        Task.FromResult(MockData.Organizations.Where(o => !o.IsBlocked).ToList());

    public Task AddAsync(Organization organization)
    {
        MockData.Organizations.Add(organization);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Organization organization)
    {
        var i = MockData.Organizations.FindIndex(o => o.Id == organization.Id);
        if (i >= 0) MockData.Organizations[i] = organization;
        return Task.CompletedTask;
    }
}

public class MockRestaurantRepository : IRestaurantRepository
{
    public Task<Restaurant?> GetByIdAsync(Guid id) =>
        Task.FromResult(MockData.Restaurants.FirstOrDefault(r => r.Id == id));

    public Task<List<Restaurant>> GetByOrgIdAsync(Guid orgId) =>
        Task.FromResult(MockData.Restaurants.Where(r => r.OrgId == orgId).ToList());

    public Task<List<Restaurant>> GetAllActiveAsync() =>
        Task.FromResult(MockData.Restaurants.Where(r => r.IsActive).ToList());

    public Task AddAsync(Restaurant restaurant)
    {
        MockData.Restaurants.Add(restaurant);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Restaurant restaurant)
    {
        var i = MockData.Restaurants.FindIndex(r => r.Id == restaurant.Id);
        if (i >= 0) MockData.Restaurants[i] = restaurant;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        MockData.Restaurants.RemoveAll(r => r.Id == id);
        return Task.CompletedTask;
    }
}

public class MockMenuRepository : IMenuRepository
{
    public Task<MenuItem?> GetByIdAsync(Guid id) =>
        Task.FromResult(MockData.MenuItems.FirstOrDefault(m => m.Id == id));

    public Task<List<MenuItem>> GetByOrgIdAsync(Guid orgId) =>
        Task.FromResult(MockData.MenuItems.Where(m => m.OrgId == orgId).ToList());

    public Task AddAsync(MenuItem item)
    {
        MockData.MenuItems.Add(item);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(MenuItem item)
    {
        var i = MockData.MenuItems.FindIndex(m => m.Id == item.Id);
        if (i >= 0) MockData.MenuItems[i] = item;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        MockData.MenuItems.RemoveAll(m => m.Id == id);
        return Task.CompletedTask;
    }
}

public class MockCourierRepository : ICourierRepository
{
    public Task<Courier?> GetByIdAsync(Guid id) =>
        Task.FromResult(MockData.Couriers.FirstOrDefault(c => c.Id == id));

    public Task<Courier?> GetByUserIdAsync(Guid userId) =>
        Task.FromResult(MockData.Couriers.FirstOrDefault(c => c.UserId == userId));

    public Task<List<Courier>> GetAllOnShiftAsync() =>
        Task.FromResult(MockData.Couriers.ToList());

    public Task AddAsync(Courier courier)
    {
        MockData.Couriers.Add(courier);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Courier courier)
    {
        var i = MockData.Couriers.FindIndex(c => c.Id == courier.Id);
        if (i >= 0) MockData.Couriers[i] = courier;
        return Task.CompletedTask;
    }
}

public class MockOrderRepository : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id) =>
        Task.FromResult(MockData.Orders.FirstOrDefault(o => o.Id == id));

    public Task<Order?> GetByIdWithItemsAsync(Guid id) =>
        Task.FromResult(MockData.Orders.FirstOrDefault(o => o.Id == id));

    public Task<List<Order>> GetByCustomerIdAsync(Guid customerId) =>
        Task.FromResult(MockData.Orders.Where(o => o.CustomerId == customerId).ToList());

    public Task<List<Order>> GetByOrgIdAsync(Guid orgId) =>
        Task.FromResult(MockData.Orders.Where(o => o.OrgId == orgId).ToList());

    public Task<List<Order>> GetAvailableForCouriersAsync() =>
        Task.FromResult(MockData.Orders
            .Where(o => o.Status == OrderStatus.ReadyForPickup && o.CourierId == null)
            .ToList());

    public Task<List<Order>> GetByCourierIdAsync(Guid courierId) =>
        Task.FromResult(MockData.Orders.Where(o => o.CourierId == courierId).ToList());

    public Task AddAsync(Order order)
    {
        MockData.Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Order order)
    {
        var i = MockData.Orders.FindIndex(o => o.Id == order.Id);
        if (i >= 0) MockData.Orders[i] = order;
        return Task.CompletedTask;
    }

    // Атомарность не нужна в моке — один поток
    public Task<bool> TryAcceptOrderAsync(Guid orderId, Guid courierId)
    {
        var order = MockData.Orders.FirstOrDefault(
            o => o.Id == orderId && o.Status == OrderStatus.ReadyForPickup && o.CourierId == null);

        if (order == null) return Task.FromResult(false);

        order.CourierId = courierId;
        order.Status = OrderStatus.InDelivery;
        order.AcceptedAt = DateTime.UtcNow;
        return Task.FromResult(true);
    }
}
