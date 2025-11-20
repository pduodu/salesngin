namespace salesngin.Models
{
    public class Stock : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Reference")]
        public string Reference { get; set; }

        [Display(Name = "Stock Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
        public DateTime? StockDate { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Display(Name = "Approved By Id")]
        public int? ApprovedById { get; set; }

        [ForeignKey("ApprovedById")]
        public ApplicationUser ApprovedByUser { get; set; }

        public List<StockItem> StockItems { get; set; }
    }
}
