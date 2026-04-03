using DeliveryAggregator.DTOs;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Enums;
using DeliveryAggregator.Repositories.Interfaces;
using DeliveryAggregator.Services.Interfaces;

namespace DeliveryAggregator.Services;

public class ModeratorService : IModeratorService
{
    private readonly IApplicationRepository _applications;
    private readonly IUserRepository _users;
    private readonly ICourierRepository _couriers;
    private readonly IOrganizationRepository _orgs;
    private readonly IOrderRepository _orders;

    public ModeratorService(
        IApplicationRepository applications,
        IUserRepository users,
        ICourierRepository couriers,
        IOrganizationRepository orgs,
        IOrderRepository orders)
    {
        _applications = applications;
        _users = users;
        _couriers = couriers;
        _orgs = orgs;
        _orders = orders;
    }

    public async Task<List<ApplicationResponse>> GetAllApplicationsAsync()
    {
        var apps = await _applications.GetAllAsync();
        return apps.Select(MapApp).ToList();
    }

    public async Task<List<ApplicationResponse>> GetPendingApplicationsAsync()
    {
        var apps = await _applications.GetAllPendingAsync();
        return apps.Select(MapApp).ToList();
    }

    public async Task ReviewApplicationAsync(Guid applicationId, ReviewApplicationRequest request)
    {
        var app = await _applications.GetByIdAsync(applicationId)
            ?? throw new KeyNotFoundException("Заявка не найдена");

        app.Status = request.Status;
        app.ModeratorNote = request.ModeratorNote;
        app.ReviewedAt = DateTime.UtcNow;
        await _applications.UpdateAsync(app);
    }

    // Модератор вручную создаёт пользователя после одобрения заявки
    public async Task<ModeratorUserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var existing = await _users.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new InvalidOperationException("Email уже занят");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            ContactInfo = request.ContactInfo,
            Role = request.Role
        };
        await _users.AddAsync(user);

        if (request.Role == UserRole.Courier)
        {
            var courier = new Courier
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                WorkZone = request.WorkZone
            };
            await _couriers.AddAsync(courier);
        }
        else if (request.Role == UserRole.OrganizationOwner)
        {
            var org = new Organization
            {
                Id = Guid.NewGuid(),
                OwnerId = user.Id,
                Name = request.DisplayName
            };
            await _orgs.AddAsync(org);
        }

        // Помечаем заявку как одобренную если передан ApplicationId
        if (request.ApplicationId != Guid.Empty)
        {
            var app = await _applications.GetByIdAsync(request.ApplicationId);
            if (app != null)
            {
                app.Status = ApplicationStatus.Approved;
                app.ReviewedAt = DateTime.UtcNow;
                await _applications.UpdateAsync(app);
            }
        }

        return new ModeratorUserResponse(
            user.Id, user.Email, user.DisplayName,
            user.ContactInfo, user.Role.ToString(),
            user.IsActive, user.CreatedAt
        );
    }

    public async Task<List<ModeratorCourierResponse>> GetAllCouriersAsync()
    {
        var couriers = await _couriers.GetAllOnShiftAsync(); // возвращает всех, не только на смене
        return couriers.Select(c => new ModeratorCourierResponse(
            c.Id, c.UserId,
            c.User?.DisplayName ?? "",
            c.User?.Email ?? "",
            c.WorkZone, c.IsOnShift, c.IsBlocked, c.CurrentOrderId
        )).ToList();
    }

    public async Task<List<ModeratorOrgResponse>> GetAllOrganizationsAsync()
    {
        var orgs = await _orgs.GetAllActiveAsync();
        return orgs.Select(o => new ModeratorOrgResponse(
            o.Id, o.Name,
            o.Owner?.Email ?? "",
            o.Owner?.DisplayName ?? "",
            o.IsBlocked,
            o.Restaurants.Count,
            o.CreatedAt
        )).ToList();
    }

    public async Task BlockCourierAsync(Guid courierId, BlockRequest request)
    {
        var courier = await _couriers.GetByIdAsync(courierId)
            ?? throw new KeyNotFoundException("Курьер не найден");

        courier.IsBlocked = request.IsBlocked;
        await _couriers.UpdateAsync(courier);
    }

    public async Task BlockOrganizationAsync(Guid orgId, BlockRequest request)
    {
        var org = await _orgs.GetByIdAsync(orgId)
            ?? throw new KeyNotFoundException("Организация не найдена");

        org.IsBlocked = request.IsBlocked;
        await _orgs.UpdateAsync(org);
    }

    public async Task<List<OrderResponse>> GetAllOrdersAsync()
    {
        // Модератор видит все заказы — фильтрация на стороне репозитория
        var orders = await _orders.GetByCustomerIdAsync(Guid.Empty); // TODO: Бек 2 добавит GetAllAsync
        return orders.Select(OrderService.MapOrder).ToList();
    }

    private static ApplicationResponse MapApp(RegistrationApplication a) => new(
        a.Id, a.Email, a.DisplayName,
        a.Role.ToString(), a.Status.ToString(),
        a.ModeratorNote, a.CreatedAt, a.ReviewedAt
    );
}
