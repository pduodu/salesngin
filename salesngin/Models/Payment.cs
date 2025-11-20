namespace salesngin.Models;

public class Payment : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Payment Code")]
    public string PaymentCode { get; set; }

    [Display(Name = "Sale")]
    public int SaleId { get; set; }

    [ForeignKey("SaleId")]
    public Sale Sale { get; set; }

    [Display(Name = "Payment Type")]
    public string PaymentType { get; set; }

    [Display(Name = "Amount Paid")]
    [Precision(18, 2)]
    public decimal AmountPaid { get; set; }

    [Display(Name = "Change Given")]
    [Precision(18, 2)]
    public decimal? Change { get; set; }

    [Display(Name = "Mobile Money Number")]
    public string MomoNumber { get; set; }

    [Display(Name = "Transaction Number")]
    public string TransactionNumber { get; set; }

    [Display(Name = "Payment Reference")]
    public string PaymentReference { get; set; }

    [Display(Name = "Payment Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
    public DateTime? PaymentDate { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; }
}
