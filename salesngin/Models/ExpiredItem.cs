namespace salesngin.Models;

public class ExpiredItem : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Item")]
    public int? ItemId { get; set; }

    [ForeignKey("ItemId")]
    public Models.Item Item { get; set; }

    [Display(Name = "Refund Item")]
    public int? RefundItemId { get; set; }

    [ForeignKey("RefundItemId")]
    public RefundItem RefundItem { get; set; }

    [Display(Name = "Quantity")]
    public int Quantity { get; set; }

    [Display(Name = "Old Quantity")]
    public int OldQuantity { get; set; }

    [Display(Name = "Cost")]
    [Precision(18, 2)]
    public decimal? CostPrice { get; set; }

    [Display(Name = "Total Cost")]
    [Precision(18, 2)]
    public decimal? TotalCost { get; set; }

    [Display(Name = "Expiry Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? ExpiryDate { get; set; }

    [Display(Name = "Date Recorded")]
    public DateTime DateRecorded { get; set; }

    [Display(Name = "Recorded By")]
    public string RecordedBy { get; set; }

}

