namespace salesngin.ViewModels
{
    public class CategoryViewModel : BaseViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; }

        [Display(Name = "Parent")]
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public Category Parent { get; set; }

        [Display(Name = "Can Delete")]
        public bool IsDeletable { get; set; } = false;

        [Display(Name = "Is Parent")]
        public bool IsParent { get; set; }

        //[Display(Name = "Photo")]
        //public string PhotoPath { get; set; }

        public Category Category { get; set; }

        public ICollection<Category> Categories { get; set; }
        public ICollection<Category> CategoryParents { get; set; }
        public ICollection<CategoryViewModel> CategoryParentsViewModel { get; set; }

        public List<IGrouping<int?, Category>> CategoryGroup { get; set; }

        //[NotMapped]
        //[Display(Name = "Photo")]
        //public IFormFile RecordPhoto { get; set; }

        //public virtual string CategoryPhotoPath { get => PhotoPath != null ? $"{FileStorePath.ImageDirectory}{PhotoPath}?c=" + DateTime.UtcNow : FileStorePath.noProductPhotoPath + "?c=" + DateTime.UtcNow; }


        //[NotMapped]
        //[Display(Name = "Excel File")]
        //[DataType(DataType.Upload)]
        //[AllowedExtensions([".xlsx", ".xls"], ErrorMessage = "Only Excel files allowed.")]
        //public IFormFile ExcelFile { get; set; }
        [Display(Name = "Photo")]
        public IFormFile RecordPhoto { get; set; }

        [Display(Name = "Excel Data")]
        public string ExcelData { get; set; }
        public string OperationType { get; set; }

        public int[] SelectedData { get; set; }
        public string SubCategories { get; set; }
    }
}
