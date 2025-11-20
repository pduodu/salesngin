namespace salesngin.ViewModels
{
    public class RequestSummaryViewModel
    {
        public int? RequestId { get; set; }
        public RequestItem RequestItem { get; set; }
        public int? TotalRequested { get; set; }
        public int? TotalAvailable { get; set; }
    }
}
