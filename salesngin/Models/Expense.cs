using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace salesngin.Models
{
    [Table("Expenses")]
    public class Expense : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public ExpenseCategory Category { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Date Incurred")]
        public DateTime DateIncurred { get; set; }

        [StringLength(500)]
        public string AttachmentPath { get; set; }
    }
}
