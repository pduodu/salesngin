namespace salesngin.Models;

public class AuditLog
{
    [Key]
    public int Id { get; set; }
    public string ActionType { get; set; }
    public string ActionDescription { get; set; }
    public DateTime? ActionDate { get; set; }
    public int ActionById { get; set; }
    public string ActionByFullname { get; set; }
    public string ActionInfo { get; set; }
    public int? RecordId { get; set; }

}

