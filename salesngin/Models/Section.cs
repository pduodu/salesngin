namespace salesngin.Models
{
    public class Section : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Section")]
        public string SectionName { get; set; }

        [Display(Name = "Head Of Section")]
        public int? HeadOfSectionId { get; set; }

        [ForeignKey("HeadOfSectionId")]
        public ApplicationUser HeadOfSection { get; set; }

        //Each Section has one department
        [Display(Name = "DepartmentId")]
        public int? DepartmentId { get; set; }

        //Navigation Property for Department
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        //Section has units
        public IEnumerable<Unit> Units { get; set; }
    }
}
