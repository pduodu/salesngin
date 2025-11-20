namespace salesngin.Models;

public class Customer : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Name")]
    public string CustomerName { get; set; }

    [Display(Name = "Number")]
    public string CustomerNumber { get; set; }

    [Display(Name = "Email")]
    public string CustomerEmail { get; set; }

    [Display(Name = "Company")]
    public string CompanyName { get; set; }
}
