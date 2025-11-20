namespace salesngin.Models
{
    public class StockItem : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Stock")]
        public int? StockId { get; set; }

        [ForeignKey("StockId")]
        public Stock Stock { get; set; }

        [Display(Name = "Item")]
        public int? ItemId { get; set; }

        [ForeignKey("ItemId")]
        public Item Item { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Date Added")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}", NullDisplayText = "No Date")]
        public DateTime? StockDate { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
    }
}
