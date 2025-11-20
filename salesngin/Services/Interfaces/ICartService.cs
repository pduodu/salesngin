namespace salesngin.Services.Interfaces;

public interface ICartService
{

    // Core Cart Operations (Sync)
    void RemoveFromCart(int itemId);
    void DecreaseQuantity(int itemId, int qty = 1);
    Task<bool> IncreaseQuantityAsync(int itemId, int quantity);

    // Core Cart Operations (Async + stock validation)
    //Task<bool> AddToCartAsync(int itemId, int qty);
    Task<CartUpdateResult> AddToCartAsync(int itemId, string orderType, int quantity);
    Task<CartUpdateResult> UpdateQuantityAsync(int itemId, int quantity);
    Task<CartUpdateResult> IncreaseCartQuantityAsync(int itemId, int quantity = 1);
    Task<CartUpdateResult> DecreaseCartQuantityAsync(int itemId, int quantity = 1);
    CartUpdateResult RemoveFromCartAsync(int itemId);
    CartUpdateResult EmptyCart();
    void ClearCart();
    

    // Retrieval (Sync)
    List<CartItem> GetCartItems();
    int GetCartCount();
    //decimal GetCartTotal();


    // DB-related (Async, only where needed)
    Task<List<Inventory>> GetCartProductsAsync();
    Task<List<Inventory>> GetInventoryItemsAsync(string itemType = null, CancellationToken ct = default);
    Task<List<Inventory>> GetCartItemsAsync(List<CartItem> cartItems);

    //Task<CartUpdateResult> GetCartAsync();
    // Task<CartUpdateResult> AddProductToCartAsync(int itemId);
    // CartUpdateResult RemoveFromCart(int itemId);
    // CartUpdateResult DecreaseQuantity(int itemId);
    //Task<CartUpdateResult> UpdateQuantityAsync(int itemId, int quantity);
    // Task<CartUpdateResult> ClearCartAsync();
    // Task<bool> ValidateOrderQuantityAsync(int itemId, int quantity = 1);
}