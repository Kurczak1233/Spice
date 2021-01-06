using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spice.Models
{
    public class Subcategory
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Subcategory name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Category")]
        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
    }
}