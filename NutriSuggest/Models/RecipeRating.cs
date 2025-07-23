// Models/RecipeRating.cs
using System.ComponentModel.DataAnnotations;

namespace NutriSuggest.Models
{
    public class RecipeRating
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;   

        [Required]
        public string RecipeTitle { get; set; } = string.Empty;

        public int Rating { get; set; }

        // navigation (optional)
        public ApplicationUser? User { get; set; }
    }
}
