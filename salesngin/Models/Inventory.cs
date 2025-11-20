namespace salesngin.Models;

public class Inventory : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Item")]
    public int? ItemId { get; set; }

    [ForeignKey("ItemId")]
    public Item Item { get; set; }

    [Display(Name = "Retail Price")]
    [Precision(18, 2)]
    public decimal RetailPrice { get; set; }

    [Display(Name = "Wholesale Price")]
    [Precision(18, 2)]
    public decimal WholesalePrice { get; set; }

    [Display(Name = "Cost Price")]
    [Precision(18, 2)]
    public decimal CostPrice { get; set; }

    [Display(Name = "Units Per Pack")]
    public int UnitsPerPack { get; set; } // Number of individual units in a pack

    [Display(Name = "Units Per Carton")]
    public int UnitsPerCarton { get; set; } // Number of individual units in a carton

    [Display(Name = "Quantity")]
    public int Quantity { get; set; } // Current stock level (in pieces)

    [Display(Name = "Restock Level")]
    public int RestockLevel { get; set; } // Minimum level to trigger reorder

    [Display(Name = "Item Status")]
    public string Status { get; set; } //Available, Out of Stock, Low Stock).

    [NotMapped]
    public string StockItemStatus
    {
        get
        {
            if (Quantity <= 0) return StockStatus.OutOfStock;
            if (Quantity <= RestockLevel) return StockStatus.LowStock;
            return StockStatus.Available;
        }
    }

    // [Display(Name = "Item Condition")]
    // public string Condition { get; set; } //NEW, GOOD, FAIR, POOR, FAULTY) – Quick visual indicator for technicians.
    // [Display(Name = "Item Status")]
    // public string ItemStatus { get; set; } //AVAILABLE, ASSIGNED, UNDER MAINTENANCE, RETIRED) – Tracks if the item is available for use or needs attention.

    [Display(Name = "Unit of Measurement")]
    public string UnitOfMeasurement { get; set; }

    [Display(Name = "Serial Number")]
    public string SerialNumber { get; set; }

    [Display(Name = "Tag")]
    public string Tag { get; set; }

    [NotMapped]
    public int TotalPacks => UnitsPerPack > 0 ? Quantity / UnitsPerPack : 0; // Full packs in stock
    [NotMapped]
    public int RemainingPieces => UnitsPerPack > 0 ? Quantity % UnitsPerPack : Quantity; // Pieces not making up a full pack
    [NotMapped]
    public decimal RetailProfit => RetailPrice - CostPrice; // Profit per unit at retail price
    [NotMapped]
    public decimal TotalRetailProfit => RetailProfit * Quantity; // Total profit if all stock sold at retail price
    [NotMapped]
    public decimal WholesaleProfit => WholesalePrice - CostPrice; // Profit per unit at wholesale price
    [NotMapped]
    public decimal TotalWholesaleProfit => WholesaleProfit * Quantity; // Total profit if all stock sold at wholesale price
    [NotMapped]
    public decimal StockValueAtCost => CostPrice * Quantity; // Total stock value at cost price
    [NotMapped]
    public decimal StockValueAtRetail => RetailPrice * Quantity; // Total stock value at retail price
    [NotMapped]
    public decimal StockValueAtWholesale => WholesalePrice * Quantity; // Total stock value at wholesale price
    // Example display:
    // “Total stock value: GHS 2,500 at cost, GHS 3,800 at retail, projected profit: GHS 1,300”

    // <td>@inventory.Item.ItemName</td>
    // <td>@inventory.DisplayStock</td>
    // <td>@inventory.RetailPrice.ToString("C")</td>
    // <td>@inventory.RetailProfit.ToString("C")</td>
    // <td>@inventory.TotalRetailProfit.ToString("C")</td>


    // Derived property (not mapped to database)
    [NotMapped]
    public string StockQuantitySummary
    {
        get
        {
            if (UnitsPerPack <= 0)
                return $"{Quantity} pcs"; // No packaging info defined

            int packs = Quantity / UnitsPerPack; // Full packs
            int pieces = Quantity % UnitsPerPack; // Remaining pieces

            // if (packs > 0 && pieces > 0) // Mixed packs and pieces
            //     return $"{packs} packs {pieces} pcs";
            // else if (packs > 0)
            //     return $"{packs} packs";
            // else
            //     return $"{pieces} pcs";
            string packLabel = packs > 1 ? "packs" : "pack";
            string pieceLabel = pieces > 1 ? "pieces" : "piece";
            return $"{(packs > 0 ? $"{packs} {packLabel}" : "")} {(pieces > 0 ? $"{pieces} {pieceLabel}" : "")}";
        }
    }

    public void SetQuantity(int quantity)
    {
        Quantity = quantity;
        UpdateStockStatus();
    }
    public void SetRestockLevel(int restockLevel)
    {
        RestockLevel = restockLevel;
        UpdateStockStatus();
    }
    private void UpdateStockStatus()
    {
        if (Quantity <= 0)
        {
            Status = StockStatus.OutOfStock;
        }
        else if (Quantity <= RestockLevel)
        {
            Status = StockStatus.LowStock;
        }
        else
        {
            Status = StockStatus.Available;
        }
    }
}

// RetailPrice      Inventory   Prices can change depending on branch, period, or promotion — not fixed item attribute.
// WholesalePrice   Inventory   Same reason — pricing may vary over time or per location.
// CostPrice        Inventory   Cost depends on supplier, date purchased, or batch.
// UnitsPerPack     Inventory   It affects how stock is counted and sold; may differ per purchase batch.
// Quantity         Inventory   Always a stock-related dynamic value.
// RestockLevel     Inventory   Tied to inventory management, not the item definition.

//public IActionResult UpdateItem(int id, int quantity, int restockLevel)
//{
//    // Retrieve the item from the database
//    var item = _context.Items.Find(id);

//    if (item == null)
//    {
//        return NotFound();
//    }

//    // Update the quantity and restock level using the methods
//    item.SetQuantity(quantity);
//    item.SetRestockLevel(restockLevel);

//    // Save changes to the database
//    _context.Update(item);
//    _context.SaveChanges();

//    return RedirectToAction("Index");
//}

