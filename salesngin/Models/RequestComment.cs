namespace salesngin.Models
{
    public class RequestComment : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Display(Name = "Request")]
        public int? RequestId { get; set; }

        [ForeignKey("RequestId")]
        public Request Request { get; set; }


    }
}
