namespace salesngin.Models;

public class Refund : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Code")]
    public string RefundCode { get; set; }

    [Display(Name = "Original Sale")]
    public int? OriginalSaleId { get; set; }

    [ForeignKey("OriginalSaleId")]
    public Sale OriginalSale { get; set; }

    [Display(Name = "Original Order")]
    public int? OriginalOrderId { get; set; }

    [ForeignKey("OriginalOrderId")]
    public Order OriginalOrder { get; set; }

    [Display(Name = "New Sale")]
    public int? NewSaleId { get; set; }

    [Display(Name = "Refund Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? RefundDate { get; set; }

    [Display(Name = "Refund Type")]
    public string RefundType { get; set; }

    [Display(Name = "Refund Reason")]
    public string RefundReason { get; set; }

    [Display(Name = "Reason Description")]
    public string ReasonDescription { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; }

    [Display(Name = "Total Amount")]
    [Precision(18, 2)]
    public decimal? TotalRefundAmount { get; set; }

    [Display(Name = "Cash Refund Amount")]
    [Precision(18, 2)]
    public decimal? CashRefundAmount { get; set; }

    [Display(Name = "Mobile Money Refund Amount")]
    [Precision(18, 2)]
    public decimal? MobileMoneyRefundAmount { get; set; }

    [Display(Name = "Additional Payment Required")]
    [Precision(18, 2)]
    public decimal? AdditionalPaymentRequired { get; set; }

    [Display(Name = "Processed By")]
    public string ProcessedBy { get; set; }

    [Display(Name = "Notes")]
    public string Notes { get; set; }

    [Display(Name = "Refund Method")]
    public string RefundMethod { get; set; }

    [Display(Name = "Customer Name")]
    //[Required(ErrorMessage = "Please enter the name of the customer")]
    public string CustomerName { get; set; }

    [Display(Name = "Customer Number")]
    //[Required(ErrorMessage = "Please enter the contact of the customer")]
    public string CustomerNumber { get; set; }

    // Navigation Properties
    //public ICollection<OrderComment> OrderComments { get; set; } = [];
    public ICollection<RefundItem> RefundItems { get; set; } = [];
    public ICollection<RefundPayment> RefundPayments { get; set; } = [];
}

//DTOs for Refund Processing
public class RefundRequest
{
    public int OriginalSaleId { get; set; }
    public int OriginalOrderId { get; set; }
    public string RefundType { get; set; }
    public string RefundReason { get; set; }
    public string ReasonDescription { get; set; }
    public string ProcessedBy { get; set; }
    public int ProcessedById { get; set; }
    public RefundItemRequest Item { get; set; } = new RefundItemRequest();
    //public List<RefundItemRequest> Items { get; set; } = new List<RefundItemRequest>();
    [Precision(18, 2)]
    public decimal? TotalRefundAmount { get; set; }
    [Precision(18, 2)]
    public decimal? CashRefundAmount { get; set; }
    [Precision(18, 2)]
    public decimal? MobileMoneyRefundAmount { get; set; }
    public string MobileMoneyNumber { get; set; }
}

public class RefundItemRequest
{
    public int OriginalOrderItemId { get; set; }
    public int ProductId { get; set; }
    public int QuantityToRefund { get; set; }
    public string ProductCondition { get; set; }
    public string Notes { get; set; }

    // For product exchange
    public int? NewProductId { get; set; }
    public int? NewProductQuantity { get; set; }
}

public class RefundCalculationResult
{
    [Precision(18, 2)]
    public decimal TotalRefundAmount { get; set; }

    [Precision(18, 2)]
    public decimal TotalNewProductAmount { get; set; }

    [Precision(18, 2)]
    public decimal NetAmount { get; set; } // Negative = refund to customer, Positive = customer pays more
    public bool RequiresAdditionalPayment => NetAmount > 0;
    public RefundItemCalculation ItemCalculation { get; set; } = new RefundItemCalculation();
    //public List<RefundItemCalculation> ItemCalculations { get; set; } = new List<RefundItemCalculation>();
}

public class RefundItemCalculation
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductPhotoPath { get; set; }
    public int QuantityToRefund { get; set; }

    [Precision(18, 2)]
    public decimal? UnitPrice { get; set; }

    [Precision(18, 2)]
    public decimal? RefundAmount { get; set; }
    public int? NewProductId { get; set; }
    public string NewProductName { get; set; }
    public string NewProductPhotoPath { get; set; }
    public int? NewProductQuantity { get; set; }

    [Precision(18, 2)]
    public decimal? NewProductUnitPrice { get; set; }

    [Precision(18, 2)]
    public decimal? NewProductAmount { get; set; }

    [Precision(18, 2)]
    public decimal? NetItemAmount { get; set; }
}

