namespace salesngin.Models
{
    public class EmployeeType : BaseModel
    {
        [Key]
        public int Id { get; set; }
 
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<ApplicationUser> ApplicationUsers { get; set; }

    }
}
