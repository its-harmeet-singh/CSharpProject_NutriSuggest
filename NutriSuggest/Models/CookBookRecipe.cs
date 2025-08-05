using System.ComponentModel.DataAnnotations;

namespace NutriSuggest.Models
{
    public class CookBookRecipe
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Ingredients { get; set; } = ""; 

        [Required]
        public string? Instructions { get; set; }

        [Required]
        public string AuthorEmail { get; set; } = "";

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public int? Rating { get; set; }
    }
}
