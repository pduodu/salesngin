namespace salesngin.Models
{
    public class Severity : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Severity Required")]
        [Display(Name = "Severity")]
        public string SeverityText { get; set; }

        [Required(ErrorMessage = "Value Required")]
        [Display(Name = "Value")]
        public int SeverityValue { get; set; }

    }
}
