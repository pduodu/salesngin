namespace salesngin.Models;

public class DefectiveItem : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Item")]
    public int ItemId { get; set; }

    [ForeignKey("ProductId")]
    public Models.Item Item { get; set; }

    [Display(Name = "Refund Item")]
    public int? RefundItemId { get; set; }

    [ForeignKey("RefundItemId")]
    public RefundItem RefundItem { get; set; }

    [Display(Name = "Quantity")]
    public int Quantity { get; set; }

    [Display(Name = "Defect Description")]
    public string DefectDescription { get; set; }

    [Display(Name = "Date Recorded")]
    public DateTime DateRecorded { get; set; }

    [Display(Name = "Recorded By")]
    public string RecordedBy { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; } // e.g., "Pending", "Resolved", "Rejected"

    [Display(Name = "Resolution Notes")]
    public string ResolutionNotes { get; set; }

}
