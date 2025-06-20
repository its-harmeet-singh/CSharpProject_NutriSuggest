using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NutriSuggest.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Dietary flags:
        public bool IsVegetarian { get; set; }
        public bool IsGlutenFree { get; set; }

        // Navigation:
        public ICollection<FavoriteRecipe> Favorites { get; set; } = new List<FavoriteRecipe>();
    }

    public class FavoriteRecipe
    {
        public int Id { get; set; }
        [Required]
        public string RecipeTitle { get; set; } = string.Empty;

        // FK back to user
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}
