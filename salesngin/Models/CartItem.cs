namespace salesngin.Models;

public class CartItem
{
    [Key]
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string ItemCategory { get; set; } = string.Empty;
    public string ItemPhoto { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int StoreQuantity { get; set; }
    [Precision(18, 2)]
    public decimal? ItemPrice { get; set; }

    [Precision(18, 2)]
    public decimal? TotalAmount => ItemPrice * Quantity;

    public CartItem() { }

    //public CartItem(Item item)
    public CartItem(Inventory inventoryItem)
    {
        ItemId = inventoryItem.Id;
        ItemCode = inventoryItem.Item.ItemCode ?? string.Empty;
        ItemType = inventoryItem.Item.ItemType ?? string.Empty;
        ItemName = inventoryItem.Item.ItemName ?? string.Empty;
        ItemCategory = inventoryItem.Item.Category?.CategoryName ?? string.Empty;
        ItemDescription = inventoryItem.Item.ItemDescription ?? string.Empty;
        ItemPhoto = inventoryItem.Item.ItemPhotoPath ?? string.Empty;
        Quantity = 1;
        StoreQuantity = 0;
        ItemPrice = inventoryItem.RetailPrice;
    }
}
