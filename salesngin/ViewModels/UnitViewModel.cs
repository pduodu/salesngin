namespace salesngin.ViewModels
{
    public class UnitViewModel
    {

        public int Id { get; set; }

        [Required]
        [Display(Name = "Unit")]
        public string UnitName { get; set; }

        [Display(Name = "Section Id")]
        public int SectionId { get; set; }

        [ForeignKey("SectionId")]
        public Section Section { get; set; }

        public Models.Item Item { get; set; }


        [Display(Name = "Requested By")]
        public int? RequestedById { get; set; }

        public ApplicationUser RequestedBy { get; set; }

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
        public ICollection<Section> Sections { get; set; }
        public ICollection<Unit> Units { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public List<Models.Item> Items { get; set; }

    }
}
