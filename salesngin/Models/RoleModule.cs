namespace salesngin.Models
{
    public class RoleModule : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Module")]
        public int ModuleId { get; set; }

        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [ForeignKey("ModuleId")]
        public Module Module { get; set; }

        [ForeignKey("RoleId")]
        public ApplicationRole Role { get; set; }
    }
}


