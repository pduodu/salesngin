using DocumentFormat.OpenXml.EMMA;

namespace salesngin.ViewModels.OrderProcessing;

public class OrderViewModel : BaseViewModel
{
    public int Id { get; set; }
    public int? OrderId { get; set; }
    public int? SaleId { get; set; }
    [Display(Name = "Code")]
    public string OrderCode { get; set; }
    public string SalesCode { get; set; }
    public Models.Item Item { get; set; }

    [Display(Name = "Order Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? OrderDate { get; set; }

    [Display(Name = "Sales Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? SalesDate { get; set; }

    [Display(Name = "Total Charges")]
    [Precision(18, 2)]
    public decimal TotalCharges { get; set; }

    [Display(Name = "Tax Amount")]
    [Precision(18, 2)]
    public decimal VATPercent { get; set; }

    [Display(Name = "Tax Amount")]
    [Precision(18, 2)]
    public decimal TaxAmount { get; set; }

    [Display(Name = "Discount Amount")]
    [Precision(18, 2)]
    public decimal DiscountAmount { get; set; }

    [Display(Name = "SubTotal")]
    [Precision(18, 2)]
    public decimal SubTotal { get; set; }

    [Display(Name = "Total Amount")]
    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    [Display(Name = "Total Sales Amount")]
    [Precision(18, 2)]
    public decimal? TotalSalesAmount { get; set; }

    [Display(Name = "Total Sales Payments")]
    [Precision(18, 2)]
    public decimal? TotalSalesPayments { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; }

    //  --- begin : customer  ----
    [Display(Name = "Customer Id")]
    public int CustomerId { get; set; }
    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; }

    [Display(Name = "Customer Number")]
    public string CustomerNumber { get; set; }

    [Display(Name = "Customer Email")]
    public string CustomerEmail { get; set; }

    [Display(Name = "Customer Company")]
    public string CompanyName { get; set; }
    //  --- end : customer  ----

    [Display(Name = "Delivery Method")]
    public string DeliveryMethod { get; set; }

    public string PaymentMethod { get; set; }

    //PaymentDate
    [Display(Name = "Payment Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? PaymentDate { get; set; }

    public string SalesStatus { get; set; } = "Unpaid";

    [Display(Name = "Amount Received")]
    [Precision(18, 2)]
    public decimal? AmountReceived { get; set; }

    [Display(Name = "Cash Amount")]
    [Precision(18, 2)]
    public decimal? CashAmount { get; set; }

    [Display(Name = "MoMo Amount")]
    [Precision(18, 2)]
    public decimal? MoMoAmount { get; set; }

    [Display(Name = "Card Amount")]
    [Precision(18, 2)]
    public decimal? CardAmount { get; set; }

    [Display(Name = "Card Transaction Number")]
    public string CardTransNumber { get; set; }

    [Display(Name = "Mobile Money Number")]
    public string MoMoNumber { get; set; }

    [Display(Name = "Mobile Money Transaction Number")]
    public string MoMoTransNumber { get; set; }

    [Display(Name = "Mixed Cash Amount")]
    [Precision(18, 2)]
    public decimal? MixedCashAmount { get; set; }

    [Display(Name = "Mixed MoMo Amount")]
    [Precision(18, 2)]
    public decimal? MixedMoMoAmount { get; set; }

    [Display(Name = "Mobile Money Number")]
    public string MixedMoMoNumber { get; set; }


    [Display(Name = "Change Due")]
    [Precision(18, 2)]
    public decimal? Change { get; set; }

    [Display(Name = "Comment")]
    public string Comment { get; set; }

    [Display(Name = "Notification Sound")]
    public string SoundPath { get; set; }
    public string IsControllerAddAction { get; set; }
    public int ResetSearchInput { get; set; }

    //  --- begin : Refund Properties ----
    [Display(Name = "Code")]
    public string RefundCode { get; set; }
    [Column("RefundDate")]
    [Display(Name = "Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? RefundDate { get; set; }
    [Display(Name = "Refund Method")]
    public string RefundMethod { get; set; }
    //  --- end : Refund Properties ----

    public string Currency { get; set; }
    public string OrderType { get; set; }
    public string SelectedOrderType { get; set; }
    public string CurrencySymbol { get; set; }

    [Display(Name = "Shop")]
    public int? ShopId { get; set; }

    [Display(Name = "Selected Shop")]
    public int? SelectedShopId { get; set; }
    public int? HoldShopId { get; set; }
    public string HoldOrderType { get; set; }
    public SearchViewModel SearchViewModel { get; set; }
    //public Shop Shop { get; set; }
    //public List<Shop> Shops { get; set; }
    public Sale Sale { get; set; }
    public Company Company { get; set; }
    public Order Order { get; set; }
    public ApplicationSetting ApplicationSetting { get; set; }
    public List<Customer> Customers { get; set; }
    public List<Order> Orders { get; set; }
    public List<OrderItem> OrderItems { get; set; }
    public List<CartItem> CartItems { get; set; }
    public List<OrderComment> OrderComments { get; set; }
    //public Models.Item Item { get; set; }
    public List<Models.Item> Items { get; set; }
    public Inventory StoreItem { get; set; }
    public List<Inventory> StoreItems { get; set; }
    public List<Inventory> AvailableItems { get; set; }
    public List<Category> Categories { get; set; }
    public List<Status> ProductStatuses { get; set; }
    public List<Status> OrderTypes { get; set; }
    public List<Status> OrderStatuses { get; set; }
    public List<Status> StockStatuses { get; set; }
    //public List<Product> Products { get; set; }
    public Stock Stock { get; set; }
    public List<Stock> Stocks { get; set; }
    public StockItem StockItem { get; set; }
    public List<Stock> StoreProducts { get; set; }
    public List<Refund> Refunds { get; set; }
    public List<RefundItem> RefundItems { get; set; }
    public OrderCommentsViewModel OrderCommentsViewModel { get; set; }
    public List<ApplicationUser> Users { get; set; }

    //public List<Category> CategoryParents { get; set; }
   

}

