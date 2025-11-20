namespace salesngin.Models
{
    public class Department : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Department")]
        public string DepartmentName { get; set; }

        [Display(Name = "Head Of Department")]
        public int? HeadOfDepartmentId { get; set; }

        [ForeignKey("HeadOfDepartmentId")]
        public ApplicationUser HeadOfDepartment { get; set; }

        //Navigation Property
        public IEnumerable<Section> Sections { get; set; }
    }
}
