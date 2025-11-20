namespace salesngin.Services.Implementations;

public class CartService : ICartService
{
    private const string CartSessionKey = "CartItems";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _db;
    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    public CartService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext db)
    {
        _httpContextAccessor = httpContextAccessor;
        _db = db;
    }

    //private ISession Session => _httpContextAccessor.HttpContext!.Session;

    private List<CartItem> GetCart() => Session.GetJson<List<CartItem>>(CartSessionKey) ?? [];
    private void SaveCart(List<CartItem> cart) => Session.SetJson(CartSessionKey, cart);
    public void AddToCart(CartItem item)
    {
        var cart = GetCart();
        var existing = cart.FirstOrDefault(x => x.ItemId == item.ItemId);
        if (existing != null) existing.Quantity += item.Quantity; // increase qty
        else cart.Add(item);

        SaveCart(cart);
    }

    public async Task<CartUpdateResult> AddToCartAsync(int itemId, string orderType, int quantity = 1)
    {
        var result = new CartUpdateResult();
        var cart = GetCart();
        result.CartItems = cart;

        if (itemId <= 0)
        {
            result.Success = false; result.Message = "Invalid item";
            return result;
        }

        if (quantity <= 0)
        {
            result.Success = false; result.Message = "Quantity cannot be 0";
            return result;
        }

        // load required inventory info (single DB access)
        var inventory = await _db.Inventory.Include(i => i.Item).FirstOrDefaultAsync(i => i.ItemId == itemId);
        if (inventory == null)
        {
            result.Success = false; result.Message = "Item not found in inventory.";
            return result;
        }

        if (inventory.Quantity < quantity)
        {
            result.Success = false; result.Message = "Insufficient stock.";
            return result;
        }

        //return OperationResult.Fail("Not enough stock"); // Not enough stock: prevents adding more items to the cart than are available in inventory, ensuring accurate stock management
        var price = !string.IsNullOrEmpty(orderType) && orderType == OrderType.Wholesale ? inventory.WholesalePrice : inventory.RetailPrice;
        // find existing
        var existing = cart.FirstOrDefault(c => c.ItemId == itemId);
        if (existing == null)
        {
            cart.Add(new CartItem
            {
                ItemId = itemId,
                ItemName = inventory.Item.ItemName,
                ItemPhoto = inventory.Item.ItemPhotoPath,
                Quantity = quantity,
                StoreQuantity = inventory.Quantity,
                ItemPrice = price,
            });
        }
        else
        {
            if (existing.Quantity + quantity > inventory.Quantity)
            {
                result.CartItems = cart;
                result.Success = false;
                result.Message = "Would exceed stock";
                return result;
            }
            existing.Quantity += quantity;
        }

        SaveCart(cart);
        result.Success = true;
        result.Message = "";
        //result.Message = "Item added to cart";
        return result;
    }
    public async Task<CartUpdateResult> UpdateQuantityAsync(int itemId, int quantity)
    {
        var result = new CartUpdateResult();
        var cartItems = GetCart();
        result.CartItems = cartItems;

        if (itemId <= 0)
        {
            result.Success = false; result.Message = "Invalid item.";
            return result;
        }

        // read cart once
        var existing = cartItems.FirstOrDefault(c => c.ItemId == itemId);
        if (existing == null)
            return new CartUpdateResult { Success = false, Message = "Item not in cart", CartItems = cartItems };

        if (quantity <= 0)
        {
            cartItems.Remove(existing);
            SaveCart(cartItems);
            return new CartUpdateResult { Success = true, Message = "Removed from cart", CartItems = cartItems };
        }

        // fetch stock to validate
        var inventory = await _db.Inventory.AsNoTracking().FirstOrDefaultAsync(i => i.ItemId == itemId);
        if (inventory != null && quantity > inventory.Quantity)
            return new CartUpdateResult { Success = false, Message = "Not enough stock", CartItems = cartItems };

        existing.Quantity = quantity;
        SaveCart(cartItems);
        return new CartUpdateResult { Success = true, Message = "Quantity updated", CartItems = cartItems };
    }
    public async Task<CartUpdateResult> IncreaseCartQuantityAsync(int itemId, int quantity = 1)
    {
        var result = new CartUpdateResult();
        var cartItems = GetCart();
        result.CartItems = cartItems;
        if (itemId <= 0)
        {
            result.Success = false; result.Message = "Invalid item";
            return result;
        }
        // read cart once
        var existing = cartItems.FirstOrDefault(c => c.ItemId == itemId);
        if (existing == null)
        {
            result.Success = false; result.Message = "Item not in cart";
            return result;
        }
        // fetch stock to validate
        var inventory = await _db.Inventory.AsNoTracking().FirstOrDefaultAsync(i => i.ItemId == itemId);
        if (inventory == null || existing.Quantity + quantity > inventory.Quantity)
        {
            result.Success = false; result.Message = "Not enough stock";
            return result;
        }

        existing.Quantity += quantity;
        SaveCart(cartItems);
        result.Success = true; result.Message = "Quantity updated";
        return result;
    }
    public async Task<CartUpdateResult> DecreaseCartQuantityAsync(int itemId, int quantity = 1)
    {
        var result = new CartUpdateResult();
        var cartItems = GetCart();
        result.CartItems = cartItems;
        if (itemId <= 0)
        {
            result.Success = false; result.Message = "Invalid item";
            return result;
        }
        // read cart once
        var existing = cartItems.FirstOrDefault(c => c.ItemId == itemId);
        if (existing == null)
        {
            result.Success = false; result.Message = "Item not in cart";
            return result;
        }
        // fetch stock to validate
        var inventory = await _db.Inventory.AsNoTracking().FirstOrDefaultAsync(i => i.ItemId == itemId);
        if (inventory == null || existing.Quantity - quantity < 0 || existing.Quantity - quantity > inventory.Quantity)
        {
            result.Success = false; result.Message = "Not enough stock";
            return result;
        }
        else if (existing.Quantity - quantity == 0)
        {
            cartItems.Remove(existing);
            SaveCart(cartItems);
            result.Success = true; result.Message = "Item removed from cart";
            return result;
        }

        existing.Quantity -= quantity;
        SaveCart(cartItems);
        result.Success = true; result.Message = "Quantity updated";
        return result;
    }
    public CartUpdateResult RemoveFromCartAsync(int itemId)
    {
        var result = new CartUpdateResult();
        var cartItems = GetCart();
        result.CartItems = cartItems;
        if (itemId <= 0)
        {
            result.Success = false; result.Message = "Invalid item";
            return result;
        }
        // read cart once
        var item = cartItems.FirstOrDefault(x => x.ItemId == itemId);
        if (item == null)
        {
            result.Success = false; result.Message = "Item not in cart";
            return result;
        }

        cartItems.Remove(item);
        SaveCart(cartItems);
        result.Success = true; result.Message = "Item removed from cart";
        return result;
    }
    public async Task<bool> IncreaseQuantityAsync(int itemId, int quantity)
    {
        var inventory = await _db.Inventory.FirstOrDefaultAsync(i => i.ItemId == itemId);
        if (inventory == null) return false;

        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.ItemId == itemId);

        if (item == null) return false;

        if (item.Quantity + quantity > inventory.Quantity)
            return false; // exceed stock

        item.Quantity += quantity;
        SaveCart(cart);
        return true;
    }
    public void RemoveFromCart(int itemId)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(x => x.ItemId == itemId);
        if (item != null)
        {
            cart.Remove(item);
            SaveCart(cart);
        }
    }
    public void DecreaseQuantity(int itemId, int qty = 1)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(x => x.ItemId == itemId);

        if (item != null)
        {
            item.Quantity -= qty;
            if (item.Quantity <= 0)
                cart.Remove(item);

            SaveCart(cart);
        }
    }
    public CartUpdateResult EmptyCart()
    {
        Session.Remove(CartSessionKey);
        var result = new CartUpdateResult
        {
            //clear cart
            CartItems = [],
            Success = true
        };
        return result;
    }
    public void ClearCart()
    {
        Session.Remove(CartSessionKey);
    }
    public List<CartItem> GetCartItems()
    {
        return GetCart();
    }
    public int GetCartCount()
    {
        return GetCart().Sum(x => x.Quantity);
    }
    public async Task<List<Inventory>> GetCartProductsAsync()
    {
        var cart = GetCart();
        var productIds = cart.Select(c => c.ItemId).ToList();

        return await _db.Inventory.AsNoTracking()
            .Include(i => i.Item)
            .Where(i => productIds.Contains(i.Id))
            .ToListAsync();
    }
    public async Task<List<Inventory>> GetInventoryItemsAsync(string itemType = null, CancellationToken ct = default)
    {
        // Single batched reads, all AsNoTracking
        IQueryable<Inventory> query = _db.Inventory.AsNoTracking().Include(i => i.Item).Where(i => i.Quantity > 0);

        // Apply filter if itemType provided
        if (!string.IsNullOrWhiteSpace(itemType))
        {
            query = query.Where(i => i.Item.ItemType == itemType);
        }

        // Run sequentially (safe with DbContext)
        var items = await query.ToListAsync(ct);

        return items;
    }
    public async Task<List<Inventory>> GetCartItemsAsync(List<CartItem> cartItems)
    {
        List<Inventory> storeItems = [];
        foreach (CartItem item in cartItems)
        {
            Inventory product = await _db.Inventory.Include(p => p.Item).FirstOrDefaultAsync(p => p.ItemId == item.ItemId);
            if (product != null)
            {
                product.Quantity -= item.Quantity;
                storeItems.Add(product);
            }
        }
        return storeItems;
    }

}