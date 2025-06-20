using Microsoft.AspNetCore.Mvc;
using NutriSuggest.Data;
using NutriSuggest.Models;
using NutriSuggest.Services;

namespace NutriSuggest.Controllers
{
    public class MealPlannerController : Controller
    {
        private readonly ChatGptService _chat;
        private readonly ApplicationDbContext _db;

        public MealPlannerController(
            ChatGptService chat,
            ApplicationDbContext db)
        {
            _chat = chat;
            _db = db;
        }

        // GET: /MealPlanner/Index
        public IActionResult Index()
            => View();

        // POST: /MealPlanner/Suggest
        [HttpPost]
        public async Task<IActionResult> Suggest(
            List<string> ingredients,
            bool vegetarian,
            bool glutenFree)
        {
            var suggestions = await _chat
                .SuggestRecipesAsync(ingredients, vegetarian, glutenFree);
            return View("Suggestions", suggestions);
        }

        // POST: /MealPlanner/Rate
        [HttpPost]
        public IActionResult Rate(string title, int rating)
        {
            _db.RecipeRatings.Add(new RecipeRating
            {
                RecipeTitle = title,
                Rating = rating
            });
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
