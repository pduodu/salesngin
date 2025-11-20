namespace salesngin.Models
{
    public class Module
    {
        [Key]
        public int Id { get; set; }
        public string ModuleName { get; set; }
        public string ModuleDisplay { get; set; }
        public string ModuleDescription { get; set; }
    }
}
