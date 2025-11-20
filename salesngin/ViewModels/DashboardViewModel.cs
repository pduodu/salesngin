namespace salesngin.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {

        public int? OpenedHazards { get; set; }
        public int? ClosedHazards { get; set; }
        public int? CompletedHazards { get; set; }

        public int? OpenedVoluntary { get; set; }
        public int? ClosedVoluntary { get; set; }
        public int? CompletedVoluntary { get; set; }

        //public int? ActiveYear { get; set; }

        //public List<int> ActiveYears { get; set; }

        public int RequestsCount { get; set; }

        public int ItemsCount { get; set; }

        public int UsersCount {  get; set; }

        public int InventoryCount { get; set; }

        public List<RequestViewModel> Requests { get; set; }
        public List<Request> AllRequests { get; set; }
        public List<Models.Item> AllItems { get; set; }
        public List<Inventory> InventoryItems { get; set; }
        public List<Stock> AllStoreItems { get; set; }
        public List<RequestSummaryViewModel> RequestedItems { get; set; }

        //public OrderViewModel OrdersViewModel { get; set; }
        //public List<Sale> Staff { get; set; }
    }
}
