namespace salesngin.ViewModels
{
    public class RegisterViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Other Names")]
        public string OtherNames { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Staff Number")]
        public string StaffNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password does not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        public IFormFile Photo { get; set; }
        public string PhotoPath { get; set; }

        public string RoleName { get; set; }
        public IList<string> UserRole { get; set; }

        public int? Department { get; set; }

        //[Display(Name = "Location")]
        //public int? LocationID { get; set; }

        //public IEnumerable<EmployeeGroup> EmployeeGroups { get; set; }
        //[Display(Name = "Employee Group")]
        //public int? EmployeeGroupId { get; set; }

        public virtual IEnumerable<Department> Departments { get; set; }
        public virtual IEnumerable<Department> Contries { get; set; }
        public virtual IEnumerable<IdentityRole> Roles { get; set; }
        public virtual IEnumerable<ApplicationRole> ApplicationRoles { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        //public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<ApplicationUser> Employees { get; set; }
        public virtual ICollection<ApplicationUser> EmployeeList { get; set; }



    }
}
