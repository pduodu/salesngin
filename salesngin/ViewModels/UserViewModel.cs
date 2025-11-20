namespace salesngin.ViewModels
{
    public class UserViewModel : PersonViewModel
    {
        [Display(Name = "User Type")]
        public string UserType { get; set; }

        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Display(Name = "Employee Type")]
        public int? EmployeeTypeId { get; set; }

        [Display(Name = "StaffNumberName")]
        public string StaffNumberName => $"{StaffNumber} - {FullName}";

        [Display(Name = "Staff Number")]
        public string StaffNumber { get; set; }

        [Display(Name = "Company")]
        public string Company { get; set; }

        [Display(Name = "SectionId")]
        public int? SectionId { get; set; }

        [Display(Name = "UnitId")]
        public int? UnitId { get; set; }

        [Display(Name = "User")]
        public int? UserId { get; set; }

        [Display(Name = "Date Started")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? DateStarted { get; set; }

        [Display(Name = "Date Ended")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? DateEnded { get; set; }



        //Student 

        [Display(Name = "Secondary Telephone")]
        public string SecondaryTelephone { get; set; }

        [Display(Name = "Organization Type")]
        public string OrganizationType { get; set; }

        [Display(Name = "Supervisor")]
        public string SupervisorName { get; set; }

        [Display(Name = "Supervisor Job Title")]
        public string SupervisorJobTitle { get; set; }

        [Display(Name = "Supervisor Email")]
        public string SupervisorEmail { get; set; }

        [Display(Name = "Supervisor Telephone")]
        public string SupervisorNumber { get; set; }




        //Instructor




        //[NotMapped]
        [Display(Name = "Photo")]
        public IFormFile Photo { get; set; }

        [Display(Name = "Photo")]
        public IFormFile avatar { get; set; }

        public virtual Section Section { get; set; }
        public virtual Unit Unit { get; set; }
        public virtual Department Department { get; set; }
        public virtual EmployeeType EmployeeType { get; set; }
        //public virtual Employee Employee { get; set; }
        public virtual ApplicationUser Employee { get; set; }
        public virtual ApplicationUser User { get; set; }
        //public virtual ApplicationUser UserLoggedIn { get; set; }
        public virtual ApplicationRole UserRole { get; set; }

        public List<Unit> Units { get; set; }
        public List<Section> Sections { get; set; }
        public List<Department> Departments { get; set; }
        public List<ApplicationUser> Employees { get; set; }
        //public List<Employee> Employees { get; set; }
        public List<EmployeeType> EmployeeTypes { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public List<ApplicationRole> Roles { get; set; }
        public List<RoleModule> RoleModules { get; set; }
        public List<ModulePermission> RoleModulePermissions { get; set; }
        public List<ApplicationRole> UserRoles { get; set; }
        public List<Title> Titles { get; set; }
    }
}
