namespace salesngin.Models
{
    public class Title
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Title")]
        public string Name { get; set; }
    }
}
