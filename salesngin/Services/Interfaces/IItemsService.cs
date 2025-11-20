using DocumentFormat.OpenXml.EMMA;

namespace salesngin.Services.Interfaces;

public interface IItemsService
{
    // Page data for Index
    Task<ItemViewModel> GetPageBindersAsync(ItemViewModel input, string itemType = null, CancellationToken ct = default);
    Task<ItemViewModel> GetItemsPageAsync(int userId, string itemType = null, CancellationToken ct = default);
    Task<ItemViewModel> GetItemsInventoryPageAsync(int userId, string itemType = null, CancellationToken ct = default);
    Task<List<Models.Item>> GetItemsAsync(string itemType = null, CancellationToken ct = default);

    Task<ItemViewModel> GetItemDetailsPageAsync(int itemId, CancellationToken ct = default);
    Task<ItemViewModel> GetInventoryItemDetailsPageAsync(int itemId, CancellationToken ct = default);

    // Page data for Create
    Task<ItemViewModel> GetCreatePageAsync(int userId, string itemType, CancellationToken ct = default);

    // Page data for Update
    Task<ItemViewModel> GetUpdatePageAsync(int itemId, int userId, CancellationToken ct = default);
    Task<ItemViewModel> GetItemUpdatePageAsync(int itemId, CancellationToken ct = default);

    // Commands
    Task<OperationResult<int>> CreateAsync(ItemViewModel input, ApplicationUser user, CancellationToken ct = default);
    Task<OperationResult> UpdateAsync(int itemId, ItemViewModel input, ApplicationUser user, CancellationToken ct = default);
    Task<OperationResult> UpdateItemAsync(int itemId, ItemViewModel input, ApplicationUser user, CancellationToken ct = default);
    Task<OperationResult> DeleteAsync(int itemId, CancellationToken ct = default);

    // Utilities
    Task<bool> ItemCodeExistsAsync(string code, CancellationToken ct = default);
    Task<List<Inventory>> GetCartItemsAsync(List<CartItem> cartItems);
    Task<List<Category>> GetItemCategoriesByTypeAsync(string itemType, CancellationToken ct = default);
    Task<List<Category>> GetItemCategoriesAsync(CancellationToken ct = default);
    Task<List<Models.Location>> GetAllLocationsAsync(CancellationToken ct = default);
}

