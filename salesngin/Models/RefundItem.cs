namespace salesngin.Models;

public class RefundItem : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Original Order Item")]
    public int OriginalOrderItemId { get; set; }

    [ForeignKey("OriginalOrderItemId")]
    public OrderItem OriginalOrderItem { get; set; }

    [Display(Name = "Item")]
    public int? ItemId { get; set; }

    [ForeignKey("ItemId")]
    public Models.Item Item { get; set; }

    [Display(Name = "Quantity to Refund")]
    public int QuantityToRefund { get; set; }

    [Display(Name = "Unit Price")]
    [Precision(18, 2)]
    public decimal? UnitPrice { get; set; }

    [Display(Name = "Total Amount")]
    [Precision(18, 2)]
    public decimal? TotalAmount { get; set; }

    [Display(Name = "Total Price")]
    [Precision(18, 2)]
    public decimal? TotalPrice { get; set; }

    [Display(Name = "Item Condition")]
    public string ItemCondition { get; set; }

    [Display(Name = "New Item")]
    public int? NewItemId { get; set; }

    [ForeignKey("NewItemId")]
    public Models.Item NewItem { get; set; }

    [Display(Name = "New Item Quantity")]
    public int? NewItemQuantity { get; set; }

    [Display(Name = "New Item Unit Price")]
    [Precision(18, 2)]
    public decimal? NewItemUnitPrice { get; set; }

    [Display(Name = "Notes")]
    public string Notes { get; set; }


    [Display(Name = "Refund")]
    public int? RefundId { get; set; }

    [ForeignKey("RefundId")]
    public Refund Refund { get; set; }

}

