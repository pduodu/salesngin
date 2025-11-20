namespace salesngin.ViewModels
{
    public class PersonViewModel : BaseViewModel
    {
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Other Name")]
        public string OtherName { get; set; }

        [Display(Name = "Full Name")]
        public string FullName => $"{LastName} {FirstName} {OtherName}";

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Display(Name = "Country")]
        public int? CountryId { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "PhoneNumber")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Address")]
        public string PostalAddress { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Date of Birth")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? DOB { get; set; }

        [Display(Name = "Role Name")]
        public string RoleName { get; set; }

        [Display(Name = "Role Id")]
        public int? RoleId { get; set; }

        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Password and Confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Display(Name = "Photo")]
        public string PhotoPath { get; set; }
        public string UserPhotoPath { get; set; }

        [Display(Name = "Old Photo")]
        public string OldPhotoPath { get; set; }
        public List<Country> Countries { get; set; }
        public Country Country { get; set; }

        public bool IsActive { get; set; }

    }
}
