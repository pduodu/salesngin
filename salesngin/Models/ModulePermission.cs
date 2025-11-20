namespace salesngin.Models
{
    public class ModulePermission : BaseModel
    {
        [Key]
        public int Id { get; set; }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool Export { get; set; }
        public bool Configure { get; set; }
        public bool Approve { get; set; }
        public bool Appoint { get; set; }
        public bool Report { get; set; }


        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [Display(Name = "Module")]
        public int ModuleId { get; set; }

        [Display(Name = "User")]
        public int UserId { get; set; }

        [ForeignKey("RoleId")]
        public ApplicationRole Role { get; set; }

        [ForeignKey("ModuleId")]
        public Module Module { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
