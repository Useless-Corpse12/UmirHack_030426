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

    public async Task<List<RestaurantResponse>> GetAllRestaurantsAsync()
    {
        var restaurants = await _restaurants.GetAllActiveAsync();
        return restaurants.Select(MapRestaurant).ToList();
    }

    public async Task<RestaurantResponse> CreateRestaurantAsync(Guid ownerId, CreateRestaurantRequest request)
    {
        var org = await _orgs.GetByOwnerIdAsync(ownerId)
            ?? throw new InvalidOperationException("Организация не найдена");

        if (org.IsBlocked)
            throw new InvalidOperationException("Организация заблокирована");

        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(),
            OrgId = org.Id,
            Name = request.Name,
            Address = request.Address,
            Lat = request.Lat,
            Lng = request.Lng,
            DeliveryRadius = request.DeliveryRadius
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
