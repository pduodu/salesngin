using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace salesngin.Models
{
    [Table("ExpenseCategories")]
    public class ExpenseCategory : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Category")]
        [StringLength(150)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
