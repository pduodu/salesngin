namespace salesngin.Models
{
    public class Request : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Code")]
        public string RequestCode { get; set; }

        [Display(Name = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
        public DateTime? RequestDate { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Reference")]
        public string Reference { get; set; }

        [Display(Name = "Requested By")]
        public int? RequestedById { get; set; }

        [ForeignKey("RequestedById")]
        public ApplicationUser RequestedBy { get; set; }

        [Display(Name = "Approved By")]
        public int? ApprovedById { get; set; }

        [ForeignKey("ApprovedById")]
        public ApplicationUser ApprovedBy { get; set; }

        [Display(Name = "Supplied By")]
        public int? SuppliedById { get; set; }

        [ForeignKey("SuppliedById")]
        public ApplicationUser SuppliedBy { get; set; }

        [Display(Name = "Received By")]
        public int? ReceivedById { get; set; }

        [ForeignKey("ReceivedById")]
        public ApplicationUser ReceivedBy { get; set; }

        [Display(Name = "Date Received")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
        public DateTime? DateReceived { get; set; }

        [Display(Name = "Purpose")]
        public string Purpose { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
        public List<RequestItem> RequestItems { get; set; }
        public List<RequestComment> RequestComments { get; set; }
        public int? ItemId { get; internal set; }
    }
}
