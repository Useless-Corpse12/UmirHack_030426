using DeliveryAggregator.DTOs;
using DeliveryAggregator.Enums;

namespace DeliveryAggregator.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterCustomerAsync(RegisterCustomerRequest request);
}

public interface IApplicationService
{
    Task<ApplicationResponse> CreateAsync(CreateApplicationRequest request);
    Task<ApplicationResponse?> GetByIdAsync(Guid id);
}

public interface IOrganizationService
{
    Task<OrganizationResponse?> GetMyOrganizationAsync(Guid ownerId);
    Task<List<RestaurantResponse>> GetAllRestaurantsAsync(); // для покупателя
    Task<RestaurantResponse> CreateRestaurantAsync(Guid ownerId, CreateRestaurantRequest request);
    Task<RestaurantResponse> UpdateRestaurantAsync(Guid ownerId, Guid restaurantId, UpdateRestaurantRequest request);
    Task DeleteRestaurantAsync(Guid ownerId, Guid restaurantId);
}

public interface IMenuService
{
    Task<MenuResponse> GetMenuByOrgIdAsync(Guid orgId);
    Task<MenuItemResponse> CreateItemAsync(Guid ownerId, CreateMenuItemRequest request);
    Task<MenuItemResponse> UpdateItemAsync(Guid ownerId, Guid itemId, UpdateMenuItemRequest request);
    Task DeleteItemAsync(Guid ownerId, Guid itemId);
}

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(Guid customerId, CreateOrderRequest request);
    Task<OrderResponse?> GetByIdAsync(Guid orderId, Guid requesterId, string requesterRole);
    Task<List<OrderResponse>> GetMyOrdersAsync(Guid customerId);
    Task<List<OrderResponse>> GetOrgOrdersAsync(Guid ownerId);
    Task<OrderResponse> UpdateStatusAsync(Guid orderId, Guid requesterId, string requesterRole, OrderStatus newStatus);
}

public interface ICourierService
{
    Task StartShiftAsync(Guid userId);
    Task EndShiftAsync(Guid userId);
    Task<List<CourierOrderPreviewResponse>> GetAvailableOrdersAsync(Guid userId);
    Task<CourierOrderDetailsResponse> AcceptOrderAsync(Guid userId, Guid orderId);
    Task<OrderResponse> CompleteOrderAsync(Guid userId, Guid orderId);
    Task<List<OrderResponse>> GetMyDeliveredOrdersAsync(Guid userId);
    Task<CourierOrderDetailsResponse?> GetCurrentOrderAsync(Guid userId);
}

public interface IModeratorService
{
    Task<List<ApplicationResponse>> GetAllApplicationsAsync();
    Task<List<ApplicationResponse>> GetPendingApplicationsAsync();
    Task ReviewApplicationAsync(Guid applicationId, ReviewApplicationRequest request);
    Task<ModeratorUserResponse> CreateUserAsync(CreateUserRequest request);
    Task<List<ModeratorCourierResponse>> GetAllCouriersAsync();
    Task<List<ModeratorOrgResponse>> GetAllOrganizationsAsync();
    Task BlockCourierAsync(Guid courierId, BlockRequest request);
    Task BlockOrganizationAsync(Guid orgId, BlockRequest request);
    Task<List<OrderResponse>> GetAllOrdersAsync();
}
