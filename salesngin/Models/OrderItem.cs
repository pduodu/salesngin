namespace salesngin.Models;

public class OrderItem : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Item")]
        public int? ItemId { get; set; }

        [ForeignKey("ItemId")]
        public Item Item { get; set; }

        [Display(Name = "Unit Price")]
        [Precision(18, 2)]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Total Price")]
        [Precision(18, 2)]
        public decimal? TotalPrice { get; set; }

        [Display(Name = "Order")]
        public int? OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

    }
