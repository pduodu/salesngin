namespace salesngin.Models
{
    public class Category : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; }

        [Display(Name = "Parent")]
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public Category Parent { get; set; }

        [Display(Name = "Is Parent")]
        public bool IsParent => ParentId == null;

        [Display(Name = "Can Delete")]
        public bool IsDeletable => IsParent == false; // Only non-parent categories can be deleted

    }
}
