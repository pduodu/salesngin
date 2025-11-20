namespace salesngin.ViewModels
{
    public class RequestViewModel : BaseViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Code")]
        public string RequestCode { get; set; }

        [Display(Name = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
        public DateTime? RequestDate { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Purpose")]
        public string Purpose { get; set; }

        [Display(Name = "Requested By")]
        public int? RequestedById { get; set; }

        public ApplicationUser RequestedBy { get; set; }

        [Display(Name = "Supplied By")]
        public int? SuppliedById { get; set; }

        public ApplicationUser SuppliedBy { get; set; }

        [Display(Name = "Received By")]
        public int? ReceivedById { get; set; }


        public ApplicationUser ReceivedBy { get; set; }

        [Display(Name = "Date Received")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
        public DateTime? DateReceived { get; set; }

        public string ItemType { get; set; }
        public Models.Item Item { get; set; }
        public List<RequestItem> RequestItems { get; set; }
        public List<RequestComment> RequestComments { get; set; }

        [Display(Name = "Reference")]
        public string Reference { get; set; }

        [Display(Name = "Stock Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
        public DateTime? StockDate { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
        public string SummaryRemarks { get; set; }

        [Display(Name = "Approved By Id")]
        public int? ApprovedById { get; set; }
        public ApplicationUser ApprovedByUser { get; set; }

        public SearchViewModel SearchViewModel { get; set; }
        public Stock Stock { get; set; }
        public List<Stock> Stocks { get; set; }
        public StockItem StockItem { get; set; }
        public List<StockItem> StockItems { get; set; }
        public Request Request { get; set; }
        public List<Request> Requests { get; set; }
        public List<Models.Item> Items { get; set; }
        public Inventory StoreItem { get; set; }
        public List<Inventory> StoreItems { get; set; }
        public List<Inventory> AvailableItems { get; set; }
        public List<Category> Categories { get; set; }
        public List<Status> StockStatuses { get; set; }
        public List<CartItem> CartItems { get; set; }
        public Unit Unit { get; set; }
        public List<Unit> Units { get; set; }
        public List<ApplicationUser> Users { get; set; }
        //public int? ActiveYear { get; set; }
        //public List<int> ActiveYears { get; set; }

        
    }
}