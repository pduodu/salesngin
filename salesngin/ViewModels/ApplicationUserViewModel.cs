namespace salesngin.ViewModels
{
    public class ApplicationUserViewModel : BaseViewModel
    {
        public int Id { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Other Name")]
        public string OtherName { get; set; }

        [Display(Name = "Full name")]
        public string FullName => LastName + " " + FirstName + " " + OtherName;

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Staff Number")]
        public string StaffNumber { get; set; }

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Section")]
        public int? SectionId { get; set; }

        [NotMapped]
        [Display(Name = "Unit")]
        public int? UnitId { get; set; }

        //Displays
        [NotMapped]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }

        [NotMapped]
        [Display(Name = "Section Name")]
        public string SectionName { get; set; }

        [NotMapped]
        [Display(Name = "Unit Name")]
        public string UnitName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }


        [Display(Name = "Photo")]
        public string PhotoPath { get; set; }

        [Display(Name = "Old Photo")]
        public string OldPhotoPath { get; set; }

        public IFormFile EmployeePhoto { get; set; }

        public int? EmployeeTypeId { get; set; }

        public bool IsMaster { get; set; } = false;

        //Navigations

        public Department Department { get; set; }

        public Section Section { get; set; }


        [NotMapped]
        public string RoleName { get; set; }

        public ICollection<Department> Departments { get; set; }
        public ICollection<Section> Sections { get; set; }
        public ICollection<Unit> Units { get; set; }
        public ICollection<EmployeeType> EmployeeTypes { get; set; }
        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<ApplicationRole> ApplicationRoles { get; set; }

    }
}
