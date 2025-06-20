using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NutriSuggest.Models
{
    public class RecipeSuggestion
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public List<string> Ingredients { get; set; } = new();

        [Required]
        public List<string> Instructions { get; set; } = new();
    }
}
