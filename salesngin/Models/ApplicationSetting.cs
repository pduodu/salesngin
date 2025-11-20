namespace salesngin.Models
{
    public class ApplicationSetting : BaseModel
    {
        [Key]
        public int Id { get; set; }
        public string Name => "DEFAULT";
        public string Country { get; set; }
        public string Currency { get; set; }
        public string ReceiptMessage { get; set; }
        public string ReceiptAdvertA { get; set; }
        public string ReceiptAdvertB { get; set; }

        //Inventory Settings
        [Display(Name = "Max Stock Level Factor")]
        [Precision(18, 2)]
        public decimal? MaxStockLevelFactor { get; set; }

        public virtual Company Company { get; set; }

    }
}
