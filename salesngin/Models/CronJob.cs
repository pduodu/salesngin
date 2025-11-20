namespace salesngin.Models
{
    public class CronJob : BaseModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Time { get; set; }
        public string Intervals { get; set; }
    }
}
