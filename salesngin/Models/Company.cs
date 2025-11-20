namespace salesngin.Models
{
    public class Company : BaseModel
    {
        [Key]
        public int Id { get; set; }
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

        [Display(Name = "Settings")]
        public int? SettingId { get; set; }

        [ForeignKey("SettingId")]
        public ApplicationSetting Settings { get; set; }

    }
}
