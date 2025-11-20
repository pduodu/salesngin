namespace salesngin.ViewModels
{
    public class BaseViewModel : BaseModel
    {

        public ModulePermission ModulePermission { get; set; }
        public virtual ApplicationUser UserLoggedIn { get; set; }

        [Display(Name = "Excel File")]
        public IFormFile ExcelFile { get; set; }

        public string filterBy { get; set; }
        public int? ActiveYear { get; set; }
        public List<int> ActiveYears { get; set; }
        public int? ActiveMonth { get; set; }
        public List<(int value, string name)> ActiveMonths { get; set; }

        //Custom Pagination Properties
        public string SearchString { get; set; }
        public string CurrentFilter { get; set; }
        public string SortOrder { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public string SearchTerm { get; set; }
        public int? SelectedCategoryId { get; set; }
        public List<int> PageSizes { get; set; } // Add this property
    }
}
