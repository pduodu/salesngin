namespace salesngin.ViewModels
{
    public class ItemViewModel : BaseViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Item Id")]
        public int? ItemId { get; set; }

        [Display(Name = "Item Code")]
        public string ItemCode { get; set; }

        [Display(Name = "Item Name")]
        public string ItemName { get; set; }
        
        [Display(Name = "Item Type")]
        public string ItemType { get; set; }

        [Display(Name = "Item Description")]
        public string ItemDescription { get; set; }

        [Display(Name = "Unit of Measurement")]
        public string UnitOfMeasurement { get; set; }

        [Display(Name = "Item Photo")]
        public string ItemPhotoName { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [Display(Name = "Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int? Quantity { get; set; }

        [Display(Name = "Restock Level")]
        [Range(0, int.MaxValue, ErrorMessage = "Restock Level cannot be negative.")]
        public int? RestockLevel { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [Display(Name = "Tag")]
        public string Tag { get; set; }

        [Display(Name = "Manufacture Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
        public DateTime? ManufactureDate { get; set; }

        [Display(Name = "Expiry Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Photo")]
        public string PhotoPath { get; set; }
        public virtual string ItemPhotoPath { get => ItemPhotoName != null ? $"{FileStorePath.ItemsPhotoDirectory}{ItemPhotoName}?c=" + DateTime.UtcNow : FileStorePath.noProductPhotoPath + "?c=" + DateTime.UtcNow; }

        // [Display(Name = "Location")]
        // public int? LocationId { get; set; }

        [Display(Name = "Item Condition")]
        public string Condition { get; set; }

        [Display(Name = "Maximum Stock Level")]
        [Precision(18, 2)]
        public decimal? MaxStockLevel { get; set; }

        [Display(Name = "Stock Level Percentage")]
        [Precision(18, 2)]
        public decimal? StockLevelPercentage { get; set; }

        [Display(Name = "Restock Threshold Percentage")]
        [Precision(18, 2)]
        public decimal? RestockThresholdPercentage { get; set; }

        [Display(Name = "Retail Price")]
        [Precision(18, 2)]
        // [Range(0, double.MaxValue, ErrorMessage = "Retail Price cannot be negative.")]
        [Range(0.00, 9999999999999999.99, ErrorMessage = "Retail Price cannot be negative.")]
        public decimal? RetailPrice { get; set; }

        [Display(Name = "Wholesale Price")]
        [Precision(18, 2)]
        // [Range(0, double.MaxValue, ErrorMessage = "Wholesale Price cannot be negative.")]
        //[Range(typeof(decimal), "0", "9999999999999999.99", ErrorMessage = "Wholesale Price cannot be negative.")]
        [Range(0.00, 9999999999999999.99, ErrorMessage = "Wholesale price cannot be negative.")]
        public decimal? WholesalePrice { get; set; }

        [Display(Name = "Cost Price")]
        [Precision(18, 2)]
        // [Range(0, double.MaxValue, ErrorMessage = "Cost Price cannot be negative.")]
        [Range(0.00, 9999999999999999.99, ErrorMessage = "Cost Price cannot be negative.")]
        public decimal? CostPrice { get; set; }

        [Display(Name = "Units Per Pack")]
        [Range(0, int.MaxValue, ErrorMessage = "Units Per Pack cannot be negative.")]
        public int? UnitsPerPack { get; set; } // Number of individual units in a pack

        [Display(Name = "Units Per Carton")]
        [Range(0, int.MaxValue, ErrorMessage = "Units Per Carton cannot be negative.")]
        public int? UnitsPerCarton { get; set; } // Number of individual units in a carton

        public string ActionType { get; set; }

        [Display(Name = "Needs Restock")]
        public bool NeedsRestock { get; set; }

        [Display(Name = "New Item")]
        public bool IsNewItem { get; set; }

        // public string StockQuantitySummary
        // {
        //     get
        //     {
        //         if (UnitsPerPack.HasValue && UnitsPerPack <= 0)
        //         {
        //             return $"{Quantity} pcs"; // No packaging info defined
        //         }

        //         int packs = Quantity.Value / UnitsPerPack.Value; // Full packs
        //         int pieces = Quantity.Value % UnitsPerPack.Value; // Remaining pieces
        //         string packLabel = packs > 1 ? "packs" : "pack";
        //         string pieceLabel = pieces > 1 ? "pieces" : "piece";
        //         return $"{(packs > 0 ? $"{packs} {packLabel}" : "")} {(pieces > 0 ? $"{pieces} {pieceLabel}" : "")}";
        //     }
        // }

        public int? RemainingPieces => UnitsPerPack.HasValue && UnitsPerPack.Value > 0 ? Quantity % UnitsPerPack.Value : Quantity; // Pieces not making up a full pack
        public decimal? RetailProfit => RetailPrice - CostPrice; // Profit per unit at retail price
        public decimal? TotalRetailProfit => RetailProfit * Quantity; // Total profit if all stock sold at retail price
        public decimal? WholesaleProfit => WholesalePrice - CostPrice; // Profit per unit at wholesale price
        public decimal? TotalWholesaleProfit => WholesaleProfit * Quantity; // Total profit if all stock sold at wholesale price
        public decimal? StockValueAtCost => CostPrice * Quantity; // Total stock value at cost price
        public decimal? StockValueAtRetail => RetailPrice * Quantity; // Total stock value at retail price
        public decimal? StockValueAtWholesale => WholesalePrice * Quantity; // Total stock value at wholesale price


        public List<MaintenanceSchedule> MaintenanceSchedules { get; set; }
        public List<MaintenanceLog> MaintenanceLogs { get; set; }
        public List<Fault> Faults { get; set; }
        public List<FaultAction> FaultActions { get; set; }

        public Models.Item Item { get; set; }
        public List<Models.Item> Items { get; set; }
        public List<Category> Categories { get; set; }
        public List<Category> UnitOfMeasurements { get; set; }
        public List<Inventory> InventoryItems { get; set; }
        public Inventory InventoryItem { get; set; }
        public List<Category> CategoryParents { get; set; }
        public List<Models.Location> Locations { get; set; }
        public List<Status> ProductStatuses { get; set; }
        //public List<Status> ItemStatuses { get; set; }
        public SelectList AllLocations { get; set; }
        public SelectList ItemStatuses { get; set; }
        public SelectList FixedItemStatuses { get; set; }
        public SelectList OperationalItemStatuses { get; set; }

        public List<RequestViewModel> Requests { get; set; }
        public List<RequestItem> RequestItems { get; set; }


        //[NotMapped]
        [Display(Name = "Photo")]
        public IFormFile Photo { get; set; }
       
        [Display(Name = "Selected Month")]
        public int SelectedMonth { get; set; } = DateTime.Now.Month;
    }
}
