using System;
using System.ComponentModel.DataAnnotations;

namespace NutriSuggest.Models
{
    public class FavoriteRecipe
    {
        public int Id { get; set; }

        [Required]
        public string RecipeTitle { get; set; } = string.Empty;        

        [Required]
        public string IngredientsJson { get; set; } = string.Empty;   

        [Required]
        public string InstructionsJson { get; set; } = string.Empty;

        public DateTime FavoritedAt { get; set; } = DateTime.UtcNow;

        // FK back to user:
        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}
