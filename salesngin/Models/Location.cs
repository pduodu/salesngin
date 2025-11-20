namespace salesngin.Models;

public class Location : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Name")]
    public string LocationName { get; set; }

    [Display(Name = "Description")]
    public string Description { get; set; }

    public virtual string FullLocation => $"{LocationName} - {Description}";

    public virtual IEnumerable<Inventory> Inventories { get; set; } = new List<Inventory>();

}
