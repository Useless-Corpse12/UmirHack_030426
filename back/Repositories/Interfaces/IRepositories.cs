using DeliveryAggregator.Entities;
using DeliveryAggregator.Enums;

namespace DeliveryAggregator.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}

public interface IApplicationRepository
{
    Task<RegistrationApplication?> GetByIdAsync(Guid id);
    Task<List<RegistrationApplication>> GetAllPendingAsync();
    Task<List<RegistrationApplication>> GetAllAsync();
    Task AddAsync(RegistrationApplication application);
    Task UpdateAsync(RegistrationApplication application);
}

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid id);
    Task<Organization?> GetByOwnerIdAsync(Guid ownerId);
    Task<List<Organization>> GetAllActiveAsync();
    Task AddAsync(Organization organization);
    Task UpdateAsync(Organization organization);
}

public interface IRestaurantRepository
{
    Task<Restaurant?> GetByIdAsync(Guid id);
    Task<List<Restaurant>> GetByOrgIdAsync(Guid orgId);
    Task<List<Restaurant>> GetAllActiveAsync();
    Task AddAsync(Restaurant restaurant);
    Task UpdateAsync(Restaurant restaurant);
    Task DeleteAsync(Guid id);
}

public interface IMenuRepository
{
    Task<MenuItem?> GetByIdAsync(Guid id);
    Task<List<MenuItem>> GetByOrgIdAsync(Guid orgId);
    Task AddAsync(MenuItem item);
    Task UpdateAsync(MenuItem item);
    Task DeleteAsync(Guid id);
}

public interface ICourierRepository
{
    Task<Courier?> GetByIdAsync(Guid id);
    Task<Courier?> GetByUserIdAsync(Guid userId);
    Task<List<Courier>> GetAllOnShiftAsync();
    Task AddAsync(Courier courier);
    Task UpdateAsync(Courier courier);
}

public interface IEmailConfirmationTokenRepository
{
    Task<EmailConfirmationToken?> GetByTokenAsync(string token);
    Task<EmailConfirmationToken?> GetActiveByUserIdAsync(Guid userId);
    Task AddAsync(EmailConfirmationToken token);
    Task UpdateAsync(EmailConfirmationToken token);
    Task DeleteExpiredAsync();
}

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order?> GetByIdWithItemsAsync(Guid id);
    Task<List<Order>> GetByCustomerIdAsync(Guid customerId);
    Task<List<Order>> GetByOrgIdAsync(Guid orgId);
    Task<List<Order>> GetAvailableForCouriersAsync(); // статус ReadyForPickup, courier_id IS NULL
    Task<List<Order>> GetByCourierIdAsync(Guid courierId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);

    // Атомарное принятие заказа курьером (SELECT FOR UPDATE внутри транзакции)
    // Возвращает true если успешно, false если уже занят
    Task<bool> TryAcceptOrderAsync(Guid orderId, Guid courierId);
}
