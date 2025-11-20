namespace salesngin.Models;

public class RefundPayment : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Refund")]
        public int RefundId { get; set; }

        [ForeignKey("RefundId")]
        public Refund Refund { get; set; }

        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } // "Cash", "MobileMoney"

        [Display(Name = "Amount")]
        [Precision(18, 2)]
        public decimal? Amount { get; set; }

        [Display(Name = "Transaction Reference")]
        public string TransactionReference { get; set; }

        [Display(Name = "Mobile Money Number")]
        public string MobileMoneyNumber { get; set; }

        [Display(Name = "Payment Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }
    }
