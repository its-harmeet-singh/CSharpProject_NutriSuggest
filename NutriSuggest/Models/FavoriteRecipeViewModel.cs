using System;
using System.Collections.Generic;

namespace NutriSuggest.Models
{
    public class FavoriteRecipeViewModel
    {
        public int Id { get; set; }
        public string RecipeTitle { get; set; } = string.Empty;
        public List<string> Ingredients { get; set; } = new();
        public List<string> Instructions { get; set; } = new();
        public DateTime FavoritedAt { get; set; }
        public int? Rating { get; set; }
    }
}
