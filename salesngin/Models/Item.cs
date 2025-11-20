namespace salesngin.Models;

public class Item : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Item Code")]
    public string ItemCode { get; set; }

    [Display(Name = "Item Name")]
    public string ItemType { get; set; } // Fixed or Non-Fixed

    [Display(Name = "Item Name")]
    public string ItemName { get; set; }

    [Display(Name = "Item Description")]
    public string ItemDescription { get; set; }

    [Display(Name = "Item Photo")]
    public string ItemPhotoName { get; set; }

    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category Category { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; }

    [Display(Name = "Notes")]
    public string Notes { get; set; }

    public virtual string ItemCodeName => $"{ItemCode} - {ItemName}";
    public virtual string ItemPhotoPath { get => ItemPhotoName != null ? $"{FileStorePath.ItemsPhotoDirectory}{ItemPhotoName}?c=" + DateTime.UtcNow : FileStorePath.noProductPhotoPath + "?c=" + DateTime.UtcNow; }

    //public Inventory Inventory { get; set; }
    public virtual IEnumerable<Inventory> InventoryItems { get; set; }
    // public virtual IEnumerable<MaintenanceSchedule> MaintenanceSchedules { get; set; }
    // public virtual IEnumerable<MaintenanceLog> MaintenanceLogs { get; set; }

}
