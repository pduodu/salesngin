namespace salesngin.Models;

public class MaintenanceSchedule : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Item")]
    public int? InventoryItemId { get; set; }

    [ForeignKey("InventoryItemId")]
    public Inventory InventoryItem { get; set; }

    [Display(Name = "Interval Days")]
    public int IntervalDays { get; set; }

    [Display(Name = "Base Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? BaseDate { get; set; }

    [Display(Name = "Next Due Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? NextDueDate { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

}
