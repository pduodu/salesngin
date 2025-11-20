namespace salesngin.Models;

public class Fault : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Inventory Item")]
    public int? InventoryItemId { get; set; }   // The inventory item this fault relates to
    public Inventory InventoryItem { get; set; }    // Navigation property

    [Required]
    [StringLength(200)]
    public string Title { get; set; }   // Short fault description (e.g., "Printer not working")

    [DataType(DataType.MultilineText)]
    public string Description { get; set; }  // Detailed explanation of the fault

    [StringLength(50)]
    public string Severity { get; set; }   // Low, Medium, High, Critical

    [StringLength(50)]
    public string Status { get; set; } = FaultStatus.Open;
    // Open, In-Progress, Resolved, Closed

    [Display(Name = "Reported By")]
    public int? ReportedBy { get; set; }   // User ID or Name of reporter
   
    [ForeignKey("ReportedBy")]
    public ApplicationUser ReportedByUser { get; set; }

    public DateTime ReportedDate { get; set; } = DateTime.UtcNow;
    // Navigation property for related actions
    public ICollection<FaultAction> Actions { get; set; } = new List<FaultAction>();
}