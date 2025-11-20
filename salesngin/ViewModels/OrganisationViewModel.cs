namespace salesngin.ViewModels
{
    public class OrganisationViewModel : BaseViewModel
    {
        //Department
        public int DepartmentId { get; set; }

        [Display(Name = "Department")]
        public string DepartmentName { get; set; }

        [Display(Name = "Head Of Department")]
        public int? HeadOfDepartmentId { get; set; }

        //Section
        public int SectionId { get; set; }

        [Display(Name = "Section")]
        public string SectionName { get; set; }

        [Display(Name = "Head Of Section")]
        public int? HeadOfSectionId { get; set; }

        //Unit
        public int UnitId { get; set; }

        [Display(Name = "Unit")]
        public string UnitName { get; set; }

        [Display(Name = "Head Of Unit")]
        public int? HeadOfUnitId { get; set; }


        [Display(Name = "Value")]
        public int SeverityValue { get; set; }

       
        [Display(Name = "Description")]
        public string GroupDescription { get; set; }

        [Display(Name = "Description")]
        public string GroupAction { get; set; }

        [Display(Name = "Color Indicator")]
        public string ColorIndicatorClass { get; set; }


        public int LocationId { get; set; }

        [Display(Name = "Location")]
        public string LocationName { get; set; }

        [Display(Name = "Description")]
        public string LocationDescription { get; set; }



        //[NotMapped]
        //[Display(Name = "Excel File")]
        //[DataType(DataType.Upload)]
        //[AllowedExtensions([".xlsx", ".xls"], ErrorMessage = "Only Excel files allowed.")]
        //public IFormFile ExcelFile { get; set; }

        [Display(Name = "Excel Data")]
        public string ExcelData { get; set; }

        public int[] SelectedData { get; set; }
        public int RecordsCount { get; set; }





        //Navigations
        public Department Department { get; set; }
        public Section Section { get; set; }
        public Unit Unit { get; set; }
        public Models.Location Location { get; set; }
        public ApplicationUser HeadOfDepartment { get; set; }
        public ApplicationUser HeadOfSection { get; set; }
        public ApplicationUser HeadOfUnit { get; set; }
        public List<Department> Departments { get; set; }
        public List<Section> Sections { get; set; }
        public List<Unit> Units { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public List<Models.Location> Locations { get; set; }
        
        

    }
}
