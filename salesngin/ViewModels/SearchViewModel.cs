
namespace salesngin.ViewModels
{
    public class SearchViewModel
    {
        public List<Models.Item> Items { get; set; }
        public List<Stock> StoreProducts { get; set; }
        //public List<Warehouse> WarehouseProducts { get; set; }

        public Pager Pager { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public string SearchTerm { get; set; }


    }
}
