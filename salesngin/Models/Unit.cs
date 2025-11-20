namespace salesngin.Models
{
    public class Unit : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Unit")]
        public string UnitName { get; set; }

        public ICollection<ApplicationUser> ApplicationUsers { get; set; }

    }
}
