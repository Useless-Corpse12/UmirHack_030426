using DeliveryAggregator.DTOs;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Repositories.Interfaces;
using DeliveryAggregator.Services.Interfaces;

namespace DeliveryAggregator.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _orgs;
    private readonly IRestaurantRepository _restaurants;

    public OrganizationService(IOrganizationRepository orgs, IRestaurantRepository restaurants)
    {
        _orgs = orgs;
        _restaurants = restaurants;
    }

    public async Task<OrganizationResponse?> GetMyOrganizationAsync(Guid ownerId)
    {
        var org = await _orgs.GetByOwnerIdAsync(ownerId);
        return org == null ? null : MapOrg(org);
    }

    // Список организаций для покупателя — чтобы выбрать откуда заказывать
    public async Task<List<OrganizationListItemResponse>> GetAllOrganizationsAsync()
    {
        var orgs = await _orgs.GetAllActiveAsync();
        return orgs.Select(o => new OrganizationListItemResponse(
            o.Id,
            o.Name,
            o.Restaurants.Count(r => r.IsActive)
        )).ToList();
    }

    // Все активные рестораны (все орги)
    public async Task<List<RestaurantResponse>> GetAllRestaurantsAsync()
    {
        var restaurants = await _restaurants.GetAllActiveAsync();
        return restaurants.Select(MapRestaurant).ToList();
    }

    // Рестораны конкретной организации — покупатель выбрал оргу, смотрит её точки
    public async Task<List<RestaurantResponse>> GetRestaurantsByOrgAsync(Guid orgId)
    {
        var org = await _orgs.GetByIdAsync(orgId)
            ?? throw new KeyNotFoundException("Организация не найдена");

        if (org.IsBlocked)
            throw new InvalidOperationException("Организация заблокирована");

        var restaurants = await _restaurants.GetByOrgIdAsync(orgId);
        return restaurants
            .Where(r => r.IsActive)
            .Select(r => { r.Organization = org; return MapRestaurant(r); })
            .ToList();
    }

    public async Task<RestaurantResponse> CreateRestaurantAsync(Guid ownerId, CreateRestaurantRequest request)
    {
        var org = await _orgs.GetByOwnerIdAsync(ownerId)
            ?? throw new InvalidOperationException("Организация не найдена");

        if (org.IsBlocked)
            throw new InvalidOperationException("Организация заблокирована");

        // Без геолокации ресторан создаётся неактивным
        var hasGeo = request.Lat.HasValue && request.Lng.HasValue;

        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(),
            OrgId = org.Id,
            Name = request.Name,
            Address = request.Address,
            Lat = request.Lat,
            Lng = request.Lng,
            DeliveryRadius = request.DeliveryRadius,
            IsActive = hasGeo  // активен только если указана геолокация
        };

        await _restaurants.AddAsync(restaurant);
        restaurant.Organization = org;
        return MapRestaurant(restaurant);
    }

    public async Task<RestaurantResponse> UpdateRestaurantAsync(Guid ownerId, Guid restaurantId, UpdateRestaurantRequest request)
    {
        var org = await _orgs.GetByOwnerIdAsync(ownerId)
            ?? throw new InvalidOperationException("Организация не найдена");

        var restaurant = await _restaurants.GetByIdAsync(restaurantId)
            ?? throw new KeyNotFoundException("Ресторан не найден");

        if (restaurant.OrgId != org.Id)
            throw new UnauthorizedAccessException("Нет доступа");

        // Нельзя активировать ресторан без геолокации
        if (request.IsActive && (!request.Lat.HasValue || !request.Lng.HasValue))
            throw new InvalidOperationException("Укажите геолокацию чтобы активировать ресторан");

        restaurant.Name = request.Name;
        restaurant.Address = request.Address;
        restaurant.Lat = request.Lat;
        restaurant.Lng = request.Lng;
        restaurant.DeliveryRadius = request.DeliveryRadius;
        restaurant.IsActive = request.IsActive;

        await _restaurants.UpdateAsync(restaurant);
        restaurant.Organization = org;
        return MapRestaurant(restaurant);
    }

    public async Task DeleteRestaurantAsync(Guid ownerId, Guid restaurantId)
    {
        var org = await _orgs.GetByOwnerIdAsync(ownerId)
            ?? throw new InvalidOperationException("Организация не найдена");

        var restaurant = await _restaurants.GetByIdAsync(restaurantId)
            ?? throw new KeyNotFoundException("Ресторан не найден");

        if (restaurant.OrgId != org.Id)
            throw new UnauthorizedAccessException("Нет доступа");

        await _restaurants.DeleteAsync(restaurantId);
    }

    private static OrganizationResponse MapOrg(Organization o) => new(
        o.Id, o.Name, o.IsBlocked, o.CreatedAt,
        o.Restaurants.Select(MapRestaurant).ToList()
    );

    private static RestaurantResponse MapRestaurant(Restaurant r) => new(
        r.Id, r.OrgId,
        r.Organization?.Name ?? "",
        r.Name, r.Address, r.Lat, r.Lng, r.DeliveryRadius, r.IsActive
    );
}
