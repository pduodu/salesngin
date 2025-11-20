namespace salesngin.Models;

public class MaintenanceLog : BaseModel
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Maintenance Schedule")]
    public int? MaintenanceScheduleId { get; set; }

    [ForeignKey("MaintenanceScheduleId")]
    public MaintenanceSchedule MaintenanceSchedule { get; set; }

    [Display(Name = "Maintenance Type")]
    public string MaintenanceType { get; set; } //enum: PREVENTIVE, CORRECTIVE, INSPECTION, RELIABILITY

    [Display(Name = "Description")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } //free-text summary

    [Display(Name = "Date Performed")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    public DateTime? DatePerformed { get; set; }

    [Display(Name = "Performed By")]
    public int? PerformedBy { get; set; }

    [ForeignKey("PerformedBy")]
    public ApplicationUser PerformedByUser { get; set; }

    // [Display(Name = "Next Due Date")]
    // [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
    // public DateTime? NextDueDate { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

}
