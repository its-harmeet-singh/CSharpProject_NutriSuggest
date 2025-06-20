using System.ComponentModel.DataAnnotations;

namespace NutriSuggest.Models
{
    public class RecipeRating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string RecipeTitle { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }
    }
}
