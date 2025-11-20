namespace salesngin.ViewModels.OrderProcessing;

public class SalesViewModel : BaseViewModel
{
    public int Id { get; set; }

    [Display(Name = "Sales Code")]
    public string SalesCode { get; set; }

    [Display(Name = "Order")]
    public int? OrderId { get; set; }

    [Display(Name = "Sale")]
    public int? SaleId { get; set; }

    [Display(Name = "Shop")]
    public int? ShopId { get; set; }

    [ForeignKey("OrderId")]
    public Order Order { get; set; }

    [Column("OrderDate")]
    [Display(Name = "Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? SalesDate { get; set; }

    [Display(Name = "Payment Method")]
    public string PaymentMethod { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; }

    [Display(Name = "Code")]
    public string OrderCode { get; set; }

    [Display(Name = "Tax Percent")]
    [Precision(18, 2)]
    public decimal TaxPercent { get; set; }

    [Display(Name = "Tax Amount")]
    [Precision(18, 2)]
    public decimal TaxAmount { get; set; }

    [Display(Name = "Discount Percent")]
    [Precision(18, 2)]
    public decimal DiscountPercent { get; set; }

    [Display(Name = "Discount Amount")]
    [Precision(18, 2)]
    public decimal DiscountAmount { get; set; }

    [Display(Name = "SubTotal")]
    [Precision(18, 2)]
    public decimal SubTotal { get; set; }

    [Display(Name = "Total Amount")]
    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    [Display(Name = "Order Status")]
    public string OrderStatus { get; set; }

    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; }

    [Display(Name = "Customer Number")]
    public string CustomerNumber { get; set; }

    [Display(Name = "Delivery Method")]
    public string DeliveryMethod { get; set; }

    [Display(Name = "Total Sales")]
    [Precision(18, 2)]
    public decimal? TotalSales { get; set; }

    public int? TotalQuantity { get; set; }
    public decimal? TotalCost { get; set; }
    public decimal? TotalCostOfItems { get; set; }
    public decimal? TotalCostOfRefundedItems { get; set; }
    public decimal? TotalSold { get; set; }
    public decimal? TotalProfit { get; set; }
    public decimal? TotalCash { get; set; }
    public decimal? TotalMoMo { get; set; }

    public decimal? TotalRefundAmount { get; set; }
    public int? TotalRefundItems { get; set; }
    public decimal? TotalRefundPayments { get; set; }
    public decimal? TotalCashRefunds { get; set; }
    public decimal? TotalMoMoRefunds { get; set; }
    public decimal? TotalAdditionalPayments { get; set; }

    //[Display(Name = "Active Year")]
    //public int? ActiveYear { get; set; }
    //public List<int> ActiveYears { get; set; }
    [Display(Name = "Start Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? StartDate { get; set; }
    [Display(Name = "End Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? EndDate { get; set; }

    // Refund request
    public int OriginalSaleId { get; set; }
    public int OriginalOrderId { get; set; }
    public string RefundType { get; set; }
    public string RefundReason { get; set; }
    public string ReasonDescription { get; set; }
    public string ProcessedBy { get; set; }
    public int QuantityToRefund { get; set; }
    public string ProductCondition { get; set; }
    public string Notes { get; set; }

    public List<Refund> Refunds { get; set; }
    public List<RefundItemRequest> Items { get; set; } = [];
    public List<RefundPayment> RefundPayments { get; set; } = [];


    public Sale Sale { get; set; }
    public List<Sale> Sales { get; set; }
    public List<Payment> Payments { get; set; }
    //public List<Shop> Shops { get; set; }
    public List<Order> Orders { get; set; }
    public List<OrderItem> OrderItems { get; set; }
    public List<OrderComment> OrderComments { get; set; }
    public List<Category> Categories { get; set; }
    public List<Status> ProductStatuses { get; set; }
    public List<Status> OrderStatuses { get; set; }
    public List<Status> SalesStatuses { get; set; }
    public List<Models.Item> Products { get; set; }

    //public List<Category> CategoryParents { get; set; }
}

