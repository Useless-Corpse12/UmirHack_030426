using DeliveryAggregator.DTOs;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Enums;
using DeliveryAggregator.Repositories.Interfaces;
using DeliveryAggregator.Services.Interfaces;

namespace DeliveryAggregator.Services;

public class ModeratorService : IModeratorService
{
    private const int MaxStrikes = 3;

    private readonly IApplicationRepository _applications;
    private readonly IUserRepository _users;
    private readonly ICourierRepository _couriers;
    private readonly IOrganizationRepository _orgs;
    private readonly IOrderRepository _orders;
    private readonly IEncryptionService _encryption;

    public ModeratorService(
        IApplicationRepository applications,
        IUserRepository users,
        ICourierRepository couriers,
        IOrganizationRepository orgs,
        IOrderRepository orders,
        IEncryptionService encryption)
    {
        _applications = applications;
        _users = users;
        _couriers = couriers;
        _orgs = orgs;
        _orders = orders;
        _encryption = encryption;
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

        // Проверяем blacklist — если email уже удалён, нельзя одобрить
        if (request.Status == ApplicationStatus.Approved)
        {
            var existingUser = await _users.GetByEmailAsync(app.Email);
            if (existingUser?.IsDeleted == true)
                throw new InvalidOperationException("Email в чёрном списке — нельзя одобрить заявку");
        }

        app.Status = request.Status;
        app.ModeratorNote = request.ModeratorNote;
        app.ReviewedAt = DateTime.UtcNow;
        await _applications.UpdateAsync(app);
    }

    public async Task<ModeratorUserResponse> CreateUserAsync(CreateUserRequest request)
    {
        // Проверяем blacklist
        var existing = await _users.GetByEmailAsync(request.Email);
        if (existing != null)
        {
            if (existing.IsDeleted)
                throw new InvalidOperationException("Email в чёрном списке");
            throw new InvalidOperationException("Email уже занят");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            ContactInfo = string.IsNullOrEmpty(request.ContactInfo)
                ? null
                : _encryption.Encrypt(request.ContactInfo),
            Role = request.Role,
            IsEmailConfirmed = true // модератор создаёт — почта считается подтверждённой
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
            DecryptContact(user.ContactInfo),
            user.Role.ToString(), user.IsActive,
            user.IsDeleted, user.CreatedAt
        );
    }

    public async Task<List<ModeratorCourierResponse>> GetAllCouriersAsync()
    {
        var couriers = await _couriers.GetAllOnShiftAsync();
        return couriers.Select(c => new ModeratorCourierResponse(
            c.Id, c.UserId,
            c.User?.DisplayName ?? "",
            c.User?.Email ?? "",
            c.WorkZone, c.IsOnShift, c.IsBlocked,
            c.Strikes,
            c.CurrentOrderId
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
            o.Strikes,
            o.Restaurants.Count,
            o.CreatedAt
        )).ToList();
    }

    // Ручная блокировка — без страйков
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

    // Добавить страйк курьеру — при 3 страйках автоблокировка
    public async Task AddCourierStrikeAsync(Guid courierId, AddStrikeRequest request)
    {
        var courier = await _couriers.GetByIdAsync(courierId)
            ?? throw new KeyNotFoundException("Курьер не найден");

        courier.Strikes.Add($"[{DateTime.UtcNow:dd.MM.yyyy}] {request.Reason}");

        if (courier.Strikes.Count >= MaxStrikes)
            courier.IsBlocked = true;

        await _couriers.UpdateAsync(courier);
    }

    // Добавить страйк организации — при 3 страйках автоблокировка
    public async Task AddOrgStrikeAsync(Guid orgId, AddStrikeRequest request)
    {
        var org = await _orgs.GetByIdAsync(orgId)
            ?? throw new KeyNotFoundException("Организация не найдена");

        org.Strikes.Add($"[{DateTime.UtcNow:dd.MM.yyyy}] {request.Reason}");

        if (org.Strikes.Count >= MaxStrikes)
            org.IsBlocked = true;

        await _orgs.UpdateAsync(org);
    }

    // Уволить курьера — soft delete, email попадает в blacklist (IsDeleted = true)
    public async Task FireCourierAsync(Guid courierId, FireCourierRequest request)
    {
        var courier = await _couriers.GetByIdAsync(courierId)
            ?? throw new KeyNotFoundException("Курьер не найден");

        // Нельзя уволить если есть активный заказ
        if (courier.CurrentOrderId != null)
            throw new InvalidOperationException("Нельзя уволить курьера с активным заказом");

        var user = await _users.GetByIdAsync(courier.UserId)
            ?? throw new KeyNotFoundException("Пользователь не найден");

        // Soft delete — блокируем аккаунт и помечаем удалённым
        user.IsActive = false;
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);

        // Блокируем курьера и снимаем со смены
        courier.IsBlocked = true;
        courier.IsOnShift = false;
        courier.Strikes.Add($"[{DateTime.UtcNow:dd.MM.yyyy}] УВОЛЕН: {request.Reason}");
        await _couriers.UpdateAsync(courier);
    }

    public async Task<List<OrderResponse>> GetAllOrdersAsync()
    {
        var orders = await _orders.GetByCustomerIdAsync(Guid.Empty);
        return orders.Select(OrderService.MapOrder).ToList();
    }

    private string? DecryptContact(string? contact) =>
        string.IsNullOrEmpty(contact) ? contact : _encryption.Decrypt(contact);

    private static ApplicationResponse MapApp(RegistrationApplication a) => new(
        a.Id, a.Email, a.DisplayName,
        a.Role.ToString(), a.Status.ToString(),
        a.ModeratorNote, a.CreatedAt, a.ReviewedAt
    );
}
