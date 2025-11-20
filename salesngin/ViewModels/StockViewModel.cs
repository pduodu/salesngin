namespace salesngin.ViewModels
{
    public class StockViewModel : BaseViewModel
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

        public ApplicationUser ApprovedByUser { get; set; }


        public SearchViewModel SearchViewModel { get; set; }
        public Stock Stock { get; set; }
        public StockItem StockItem { get; set; }
        public List<StockItem> StockItems { get; set; }
        public List<Stock> Stocks { get; set; }
        public List<Models.Item> Items { get; set; }
        public List<Category> Categories { get; set; }
        public List<Status> StockStatuses { get; set; }
        //public List<OrderCartItem> OrderCartItems { get; set; }
        public List<CartItem> CartItems { get; set; }
        //public int? ActiveYear { get; set; }
        //public List<int> ActiveYears { get; set; }
    }
}
