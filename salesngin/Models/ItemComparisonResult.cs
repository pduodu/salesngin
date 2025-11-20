namespace salesngin.Models
{
    public class ItemComparisonResult
    {
        public string Differences { get; set; }
        public List<InventoryAdjustment> InventoryAdjustments { get; set; }
        public List<RequestItem> RequestedItems { get; set; }
        public List<RequestItem> RemovedItems { get; set; }

    }
}
