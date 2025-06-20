using System.ComponentModel.DataAnnotations;

namespace NutriSuggest.Models
{
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
