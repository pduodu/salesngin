namespace salesngin.ViewModels
{
    public class PaginationViewModel
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public string SortOrder { get; set; }
        public string CurrentFilter { get; set; }
        public int PageRange { get; set; } = 2;
        public string ActionUrl { get; set; }

    }
}
