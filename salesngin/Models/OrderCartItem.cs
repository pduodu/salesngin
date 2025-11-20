using DocumentFormat.OpenXml.EMMA;

namespace salesngin.Models;

public class OrderCartItem
{
    public Models.Item Item { get; set; }
    public int Quantity { get; set; }

    [Display(Name = "Cost Price")]
    [Precision(18, 2)]
    public decimal? CostPrice { get; set; }

    [Display(Name = "Selling Price")]
    [Precision(18, 2)]
    public decimal? SellingPrice { get; set; }

    [Display(Name = "Total Cost")]
    [Precision(18, 2)]
    public decimal? TotalCost => CostPrice * Quantity;

}

