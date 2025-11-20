namespace salesngin.Models;

public class Sale : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Sales Code")]
    public string SalesCode { get; set; }

    [Display(Name = "Order")]
    public int? OrderId { get; set; }

    [ForeignKey("OrderId")]
    public Order Order { get; set; }

    [Display(Name = "Sales Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? SalesDate { get; set; }

    [Display(Name = "Payment Method")]
    public string PaymentMethod { get; set; }

    [Display(Name = "Total Amount")]
    [Precision(18, 2)]
    public decimal? TotalAmount { get; set; }

    [Display(Name = "Discount Amount")]
    [Precision(18, 2)]
    public decimal? DiscountAmount { get; set; } // Discount Amount

    [Display(Name = "SubTotal Amount")]
    [Precision(18, 2)]
    public decimal? SubTotalAmount => TotalAmount - DiscountAmount; // TotalAmount - DiscountAmount

    [Display(Name = "SubTotal")]
    [Precision(18, 2)]
    public decimal? SubTotal { get; set; } // TotalAmount - DiscountAmount

    [Display(Name = "Total Charges")]
    [Precision(18, 2)]
    public decimal? TotalCharges { get; set; } // Additional charges like shipping, handling, etc.

    [Display(Name = "Total Amount With Charges")]
    [Precision(18, 2)]
    public decimal? TotalAmountWithCharges => SubTotalAmount + TotalCharges; // SubTotalAmount + TotalCharges

    [Display(Name = "VAT Percent")]
    [Precision(18, 2)]
    public decimal? VATPercent { get; set; } // VAT Percentage

    [Display(Name = "VAT Amount")]
    [Precision(18, 2)]
    public decimal? VATAmount => VATPercent / 100 * TotalAmountWithCharges ?? 0;

    [Display(Name = "Other Tax Amount")]
    [Precision(18, 2)]
    public decimal? TaxAmount { get; set; } // Other Tax Amount

    [Display(Name = "Total Amount With Charges & Taxes")]
    [Precision(18, 2)]
    public decimal? TotalAmountWithTaxesOnly => SubTotalAmount + VATAmount + TaxAmount ?? 0; // SubTotalAmount + VATAmount + TaxAmount

    [Display(Name = "Total Amount With Charges & Taxes")]
    [Precision(18, 2)]
    public decimal? TotalAmountWithChargesAndTaxes => TotalAmountWithCharges + VATAmount + TaxAmount ?? 0; // TotalAmountWithCharges + VATAmount + TaxAmount

    [Display(Name = "Status")]
    public string Status { get; set; } //Paid, Part, Due

    // Navigation property to Payments
    public ICollection<Payment> Payments { get; set; }
    public ICollection<Refund> Refunds { get; set; }

}

