using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace NutriSuggest.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Personalized tastes:
        public bool IsVegetarian { get; set; }
        public bool IsGlutenFree { get; set; }

        // Favorites navigation:
        public ICollection<FavoriteRecipe> Favorites { get; set; } = new List<FavoriteRecipe>();
    }
}
