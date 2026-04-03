using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DeliveryAggregator.Hubs;

[Authorize]
public class OrderHub : Hub
{
    // Клиент подключается и автоматически добавляется в нужные группы
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (role == "Courier")
        {
            // Курьеры подписываются на ленту доступных заказов
            await Groups.AddToGroupAsync(Context.ConnectionId, "courier-feed");
        }

        await base.OnConnectedAsync();
    }

    // Покупатель/ресторан подписывается на конкретный заказ
    public async Task SubscribeToOrder(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    public async Task UnsubscribeFromOrder(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    // Ресторан подписывается на свои заказы
    public async Task SubscribeToOrg(string orgId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"org-{orgId}");
    }
}

// События которые сервер отправляет клиентам:
// "OrderAvailable"     → курьерам (courier-feed)     { orderId, restaurantName, restaurantAddress, price }
// "OrderTaken"         → курьерам (courier-feed)     { orderId }
// "OrderStatusChanged" → покупателю (order-{id})     { orderId, status }
// "NewOrder"           → ресторану  (org-{id})        { orderId }
