namespace NutriSuggest.Models
{
    public class UserHistory
    {
        public required int Id { get; set; }

        public required string UserEmail { get; set; }

        public required string IngredientsJson { get; set; }

        public required string Title { get; set; } // e.g., "Tofu-Rice Dish"

        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
