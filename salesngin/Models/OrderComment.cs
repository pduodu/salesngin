namespace salesngin.Models;

public class OrderComment : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Comment")]
    public string Comment { get; set; }

    [Display(Name = "Order")]
    public int? OrderId { get; set; }

    [ForeignKey("OrderId")]
    public Order Order { get; set; }


}

