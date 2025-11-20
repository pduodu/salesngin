namespace salesngin.Models;
public class FaultAction : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Fault")]
    public int? FaultId { get; set; }

    [ForeignKey("FaultId")]
    public Fault Fault { get; set; }

    [Display(Name = "Action Taken")]
    [DataType(DataType.MultilineText)]
    public string ActionTaken { get; set; }  // e.g., "Replaced motherboard"

    [Display(Name = "Action Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Action By")]
    public int? ActionBy { get; set; }   // User ID or Name of reporter
    
    [ForeignKey("ActionBy")]
    public ApplicationUser ActionByUser { get; set; }

    [DataType(DataType.MultilineText)]
    public string Comments { get; set; }

    // Optional: status after this action (so you can track transitions)
    [StringLength(50)]
    public string StatusAfterAction { get; set; }
    // Open, In-Progress, Resolved, Closed


}