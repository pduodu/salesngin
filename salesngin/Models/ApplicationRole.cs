namespace salesngin.Models
{
    public class ApplicationRole : IdentityRole<int>
    {
        public string RoleDisplayText { get; set; }
        public string RoleDescription { get; set; }

        [Column("DateCreated")]
        [Display(Name = "Created")]
        public DateTime? DateCreated { get; set; }

        [Column("DateModified")]
        [Display(Name = "Modified")]
        public DateTime? DateModified { get; set; }

        [Column("CreatedBy")]
        [Display(Name = "Created By")]
        public int? CreatedBy { get; set; }

        [Column("ModifiedBy")]
        [Display(Name = "Modified By")]
        public int? ModifiedBy { get; set; }

        public bool IsDeleted { get; set; }

        public IEnumerable<ApplicationRole> Roles { get; set; }
        public IEnumerable<Module> Modules { get; set; }

        //public ICollection<ApplicationUser> UserRoles { get =>  }

    }
}
