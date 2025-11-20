using Microsoft.EntityFrameworkCore;

namespace salesngin.ViewModels
{
    public class ApplicationSettingViewModel : BaseViewModel
    {
        #region General Settings

        [Key]
        public int Id { get; set; }
        public string Name => "DEFAULT";
        public string Currency { get; set; }
        public string Country { get; set; }
        #endregion

        #region Sales Settings
        public bool DetailedChargesView { get; set; }  // True : Shows charges and their percentage and value 
        public bool ApplyCharges { get; set; }
        public bool IncludeChargesOnReceipt { get; set; }
        public bool ApplyVAT { get; set; }
        public bool IncludeVATOnReceipt { get; set; } // True : shows VAT percent and amount on receipt  (Level of Details based on DetailedChargesView above)

        [Display(Name = "VAT Percentage")]
        [Precision(18, 2)]
        public decimal? VATPercent { get; set; }

        public bool RandomizeSalesView { get; set; } //This will randomize and display sales (Not show actual sales)
        //In this example, GetRandomSales method takes a list of Sale objects as input and returns a randomized list of Sale objects.
        //The number of sales to be returned is determined by generating a random number between 20% and 50% of the total number of sales.
        //The OrderBy method is used to shuffle the list randomly, and the Take method is used to select the required number of sales.
        //public List<Sale> GetRandomSales(List<Sale> sales)
        //{
        //    Random random = new Random();
        //    int totalSales = sales.Count;
        //    int randomSalesCount = random.Next((int)(totalSales * 0.2), (int)(totalSales * 0.5));
        //    return sales.OrderBy(x => random.Next()).Take(randomSalesCount).ToList();
        //}

        [Display(Name = "Randomize From")]
        [Precision(18, 2)]
        public decimal? RandomizeSalesFrom { get; set; }
        [Display(Name = "Randomize To")]
        [Precision(18, 2)]
        public decimal? RandomizeSalesTo { get; set; }

        public string ReceiptMessage { get; set; }
        public string ReceiptAdvertA { get; set; }
        public string ReceiptAdvertB { get; set; }

        #endregion

        #region Company 
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyTIN { get; set; }
        public string CompanyEmailAddress { get; set; }
        public string CompanyPostalAddress { get; set; }
        public string CompanyPhoneNumber1 { get; set; }
        public string CompanyPhoneNumber2 { get; set; }
        public string CompanyLocation { get; set; }
        public string CompanyGPSLocation { get; set; }
        public string CompanyLogo { get; set; }
        public string BusinessDescription { get; set; }
        public virtual string CompanyLogoPath { get => CompanyLogo != null ? $"{FileStorePath.ImageDirectory}{CompanyLogo}?c=" + DateTime.UtcNow : FileStorePath.noProductPhotoPath + "?c=" + DateTime.UtcNow; }
        #endregion

        #region Charges

        [Display(Name = "Name")]
        public string ChargeName { get; set; }

        [Display(Name = "Description")]
        public string ChargeDescription { get; set; }

        [Display(Name = "Percentage")]
        [Precision(18, 2)]
        public decimal? ChargePercent { get; set; }
        public bool IsChargeActive { get; set; }

        #endregion

        //Inventory Settings
        [Display(Name = "Maximum Stock Level Factor")]
        [Precision(18, 2)]
        public decimal? MaxStockLevelFactor { get; set; }

        //[NotMapped]
        [Display(Name = "Photo")]
        public IFormFile Photo { get; set; }

        public virtual Company Company { get; set; }
        public List<Country> Countries { get; set; }

    }
}
