namespace salesngin.Models
{
    public class RequestItem : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Item")]
        public int? ItemId { get; set; }

        [ForeignKey("ItemId")]
        public Item Item { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Request")]
        public int? RequestId { get; set; }

        [ForeignKey("RequestId")]
        public Request Request { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }



    }
}
