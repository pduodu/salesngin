namespace salesngin.ViewModels
{
    public class SectionViewModel
    {

        public int Id { get; set; }

        [Required]
        [Display(Name = "Section")]
        public string SectionName { get; set; }

        //Each Section has one department
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        //Naviation Property for Department
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }


        [Column("DateCreated")]
        [Display(Name = "Created")]
        public DateTime? DateCreated { get; set; }

        [Column("DateModified")]
        [Display(Name = "Modified")]
        public DateTime? DateModified { get; set; }

        [Column("CreatedBy")]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Column("ModifiedBy")]
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        public bool IsDeleted { get; set; } = false;

        //Navigations
        public ICollection<Department> Departments { get; set; }
        public ICollection<Unit> Units { get; set; }

    }
}
