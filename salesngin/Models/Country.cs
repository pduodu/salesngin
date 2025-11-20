namespace salesngin.Models
{
    public class Country
    {
        [Key]
        public int Id { get; set; }
        public string AlphaCode2 { get; set; }
        public string AlphaCode3 { get; set; }
        public string CountryName { get; set; }
        public string Nationality { get; set; }

    }
}
