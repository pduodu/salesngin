using Microsoft.EntityFrameworkCore;

namespace salesngin.Models
{
    public class Invoice : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Number")]
        public string Number { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "From")]
        public string From { get; set; }

        [Display(Name = "To")]
        public string To { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Display(Name = "Terms")]
        public string Terms { get; set; }

        [Display(Name = "SubTotal")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Precision(18, 2)]
        public decimal? SubTotal { get; set; }

        [Display(Name = "Tax")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Precision(18, 2)]
        public decimal? TaxAmount { get; set; }

        [Display(Name = "Discount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Precision(18, 2)]
        public decimal? Discount { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Precision(18, 2)]
        public decimal? Total { get; set; }

        [Display(Name = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? Date { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }
    }
}
