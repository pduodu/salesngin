namespace salesngin.Models
{
    public class ApplicationUser : IdentityUser<int>
    {

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Other Name")]
        public string OtherName { get; set; }

        [Display(Name = "Full name")]
        public string FullName => $"{FirstName} {OtherName} {LastName}";

        [Display(Name = "Country")]
        public int? CountryId { get; set; }

        [ForeignKey("CountryId")]
        public Country Country { get; set; }

        [Display(Name = "Photo")]
        public string PhotoPath { get; set; }

        [Display(Name = "Company")]
        public string Company { get; set; }

        [Display(Name = "National ID Number")]
        public string NationalIdNumber { get; set; }

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Display(Name = "Address")]
        public string PostalAddress { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Column("DOB")]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? DOB { get; set; }

        //Stamps
        [Display(Name = "Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Modified")]
        public DateTime? DateModified { get; set; }

        [Display(Name = "Created By")]
        public int? CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public int? ModifiedBy { get; set; }

        [Column("LastLogin")]
        [Display(Name = "Last Login")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? LastLogin { get; set; }

        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool IsResetMode { get; set; } = true;

        [Display(Name = "Staff Number")]
        public string StaffNumber { get; set; }

        [Display(Name = "Employee Type")]
        public int? EmployeeTypeId { get; set; }
       
        [ForeignKey("EmployeeTypeId")]
        public EmployeeType EmployeeType { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Unit")]
        public int? UnitId { get; set; }

        [ForeignKey("UnitId")]
        public Unit Unit { get; set; }

        [ForeignKey("CreatedBy")]
        public ApplicationUser CreatedByUser { get; set; }

        [ForeignKey("ModifiedBy")]
        public ApplicationUser ModifiedByUser { get; set; }

        public virtual string UserPhotoPath { get => PhotoPath != null ? $"{FileStorePath.UserPhotoDirectory}{PhotoPath}?c=" + DateTime.UtcNow : FileStorePath.noUserPhotoPath + "?c=" + DateTime.UtcNow; }
        public virtual string UserInitials { get => $"{FirstName?[0].ToString().ToUpper()}{LastName?[0].ToString().ToUpper()}"; }
        public virtual string StaffNumberName { get => $"{StaffNumber} - {FirstName} {LastName}"; }
        public virtual string StaffUnit { get => Unit?.UnitName; }

    }
}
