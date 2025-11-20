namespace salesngin.Models;

public class Order : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Code")]
    public string OrderCode { get; set; }
    public string OrderType { get; set; }

    [Display(Name = "Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? OrderDate { get; set; }

    [Display(Name = "Total Amount")]
    [Precision(18, 2)]
    public decimal? TotalAmount { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; }

    [Display(Name = "Customer")]
    public int? CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customer Customer { get; set; }
    
    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; }

    [Display(Name = "Customer Number")]
    public string CustomerNumber { get; set; }

    [Display(Name = "Delivery Method")]
    public string DeliveryMethod { get; set; }

    [Display(Name = "Order Status")]
    public string OrderStatus { get; set; }

    public List<OrderItem> OrderItems { get; set; }
    public Sale Sale { get; set; }
    public List<OrderComment> OrderComments { get; set; }
}

public class OrderTotals
{
    [Precision(18, 2)]
    public decimal? SubTotal { get; set; }
    [Precision(18, 2)]
    public decimal? DiscountAmount { get; set; }
    [Precision(18, 2)]
    public decimal? Charges { get; set; }
    [Precision(18, 2)]
    public decimal? VatPercent { get; set; }
    [Precision(18, 2)]
    public decimal? VatAmount { get; set; }
    [Precision(18, 2)]
    public decimal? TaxAmount { get; set; }
    [Precision(18, 2)]
    public decimal? GrandTotal { get; set; }
}
