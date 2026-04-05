using DeliveryAggregator.DTOs;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Repositories.Interfaces;
using DeliveryAggregator.Services.Interfaces;

namespace DeliveryAggregator.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menu;
    private readonly IOrganizationRepository _orgs;

    public MenuService(IMenuRepository menu, IOrganizationRepository orgs)
    {
        _menu = menu;
        _orgs = orgs;
    }

    public async Task<MenuResponse> GetMenuByOrgIdAsync(Guid orgId)
    {
        var org = await _orgs.GetByIdAsync(orgId)
            ?? throw new KeyNotFoundException("Организация не найдена");

        var items = await _menu.GetByOrgIdAsync(orgId);
        var available = items.Where(i => i.IsAvailable).ToList();

        var categories = available
            .GroupBy(i => i.Category)
            .Select(g => new MenuCategoryResponse(
                g.Key,
                g.Select(MapItem).ToList()))
            .ToList();

        return new MenuResponse(orgId, org.Name, categories);
    }

    public async Task<MenuItemResponse> CreateItemAsync(Guid ownerId, CreateMenuItemRequest request)
    {
        var org = await _orgs.GetByOwnerIdAsync(ownerId)
            ?? throw new InvalidOperationException("Организация не найдена");

        var item = new MenuItem
        {
            Id = Guid.NewGuid(),
            OrgId = org.Id,
            Category = request.Category,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            PhotoUrl = request.PhotoUrl
        };

        await _menu.AddAsync(item);
        return MapItem(item);
    }

    public async Task<MenuItemResponse> UpdateItemAsync(Guid ownerId, Guid itemId, UpdateMenuItemRequest request)
    {
        var org = await _orgs.GetByOwnerIdAsync(ownerId)
            ?? throw new InvalidOperationException("Организация не найдена");

        var item = await _menu.GetByIdAsync(itemId)
            ?? throw new KeyNotFoundException("Позиция меню не найдена");

        if (item.OrgId != org.Id)
            throw new UnauthorizedAccessException("Нет доступа");

        item.Name = request.Name;
        item.Category = request.Category;
        item.Description = request.Description;
        item.Price = request.Price;
        item.PhotoUrl = request.PhotoUrl;
        item.IsAvailable = request.IsAvailable;

        await _menu.UpdateAsync(item);
        return MapItem(item);
    }

    public async Task DeleteItemAsync(Guid ownerId, Guid itemId)
    {
        var org = await _orgs.GetByOwnerIdAsync(ownerId)
            ?? throw new InvalidOperationException("Организация не найдена");

        var item = await _menu.GetByIdAsync(itemId)
            ?? throw new KeyNotFoundException("Позиция меню не найдена");

        if (item.OrgId != org.Id)
            throw new UnauthorizedAccessException("Нет доступа");

        await _menu.DeleteAsync(itemId);
    }

    private static MenuItemResponse MapItem(MenuItem i) => new(
        i.Id, i.OrgId, i.Category, i.Name,
        i.Description, i.Price, i.PhotoUrl, i.IsAvailable
    );
}
