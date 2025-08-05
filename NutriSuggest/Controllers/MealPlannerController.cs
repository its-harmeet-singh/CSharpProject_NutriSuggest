using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriSuggest.Data;
using NutriSuggest.Models;
using NutriSuggest.Services;


namespace NutriSuggest.Controllers
{
    [Authorize]
    public class MealPlannerController : Controller
    {
        private readonly ChatGptService _chat;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public MealPlannerController(
            ChatGptService chat,
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _chat = chat;
            _db = db;
            _userManager = userManager;
        }

        // GET: /MealPlanner/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? throw new InvalidOperationException("User not found");
            ViewBag.IsVegetarian = user.IsVegetarian;
            ViewBag.IsGlutenFree = user.IsGlutenFree;
            return View();
        }

        // POST: /MealPlanner/SuggestPost  (PRG pattern)
        [HttpPost]
        public IActionResult SuggestPost(
    List<string> ingredients,
    bool vegetarian,
    bool glutenFree)
        {
            TempData["IngredientsJson"] = JsonSerializer.Serialize(ingredients);
            TempData["Vegetarian"] = vegetarian;
            TempData["GlutenFree"] = glutenFree;

            // Create a redirect URL for the Suggest action
            ViewBag.RedirectUrl = Url.Action(nameof(Suggest), new
            {
                ingredients,
                vegetarian,
                glutenFree
            });

            // Return the loading view instead of immediately redirecting
            return View("Loading");
        }

        [HttpGet]
        public async Task<IActionResult> Suggest()
        {
            var ingredientsJson = TempData["IngredientsJson"] as string;
            var ingredients = string.IsNullOrEmpty(ingredientsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(ingredientsJson);

            bool vegetarian = TempData.ContainsKey("Vegetarian") && (bool)TempData["Vegetarian"];
            bool glutenFree = TempData.ContainsKey("GlutenFree") && (bool)TempData["GlutenFree"];

            // Get recipe suggestions from ChatGPT service
            var suggestions = await _chat.SuggestRecipesAsync(
                ingredients, vegetarian, glutenFree);

            var user = await _userManager.GetUserAsync(User)
                       ?? throw new InvalidOperationException("User not found");

            // ======= Generate a short and meaningful title =========
            string[] suffixes = { "Delight", "Mix", "Bowl", "Medley", "Fusion", "Surprise", "Treat", "Platter" };

            // Use first 1–2 ingredients (remove spaces)
            var shortIngredients = ingredients
                .Take(2)
                .Select(i => i.Replace(" ", "", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var random = new Random();
            string suffix = suffixes[random.Next(suffixes.Length)];
            int number = random.Next(100, 1000);

            // Combine into title
            var title = string.Join("-", shortIngredients) + " " + suffix + " #" + number;

            // ======= Store user history in database =========
            var history = new UserHistory
            {
                Id = 0,
                UserEmail = user.Email!,
                IngredientsJson = JsonSerializer.Serialize(ingredients),
                Title = title,
                CreatedAt = DateTime.UtcNow
            };

            _db.UserHistories.Add(history);
            await _db.SaveChangesAsync();

            return View("Suggestions", suggestions);
        }


        // POST: /MealPlanner/Rate
        [HttpPost]
        public async Task<IActionResult> Rate(
            string title,
            int rating,
            string returnUrl = null!)
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? throw new InvalidOperationException("User not found");

            _db.RecipeRatings.Add(new RecipeRating
            {
                UserId = user.Id,
                RecipeTitle = title,
                Rating = rating
            });
            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        // POST: /MealPlanner/Favorite
        [HttpPost]
        public async Task<IActionResult> Favorite(
            string recipeTitle,
            string ingredientsJson,
            string instructionsJson)
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? throw new InvalidOperationException("User not found");

            _db.FavoriteRecipes.Add(new FavoriteRecipe
            {
                UserId = user.Id,
                RecipeTitle = recipeTitle,
                IngredientsJson = ingredientsJson,
                InstructionsJson = instructionsJson,
                FavoritedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return RedirectToAction("MyFavorites");
        }

        // POST: /MealPlanner/DeleteFavorite
        [HttpPost]
        public async Task<IActionResult> DeleteFavorite(int id)
        {
            var fav = await _db.FavoriteRecipes.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            if (fav != null && user != null && fav.UserId == user.Id)
            {
                _db.FavoriteRecipes.Remove(fav);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("MyFavorites");
        }

        // GET: /MealPlanner/MyFavorites
        public async Task<IActionResult> MyFavorites()
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? throw new InvalidOperationException("User not found");

            var favs = await _db.FavoriteRecipes
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.FavoritedAt)
                .ToListAsync();

            var ratings = await _db.RecipeRatings
                .Where(r => r.UserId == user.Id)
                .ToDictionaryAsync(r => r.RecipeTitle, r => r.Rating);

            var model = favs.Select(f => new FavoriteRecipeViewModel
            {
                Id = f.Id,
                RecipeTitle = f.RecipeTitle,
                Ingredients = JsonSerializer.Deserialize<List<string>>(f.IngredientsJson) ?? new(),
                Instructions = JsonSerializer.Deserialize<List<string>>(f.InstructionsJson) ?? new(),
                FavoritedAt = f.FavoritedAt,
                Rating = ratings.TryGetValue(f.RecipeTitle, out var r) ? r : (int?)null
            }).ToList();

            return View(model);
        }

        // GET: /MealPlanner/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? throw new InvalidOperationException("User not found");

            var f = await _db.FavoriteRecipes
                .Where(x => x.Id == id && x.UserId == user.Id)
                .SingleOrDefaultAsync();

            if (f == null) return NotFound();

            var rating = await _db.RecipeRatings
                .Where(r => r.UserId == user.Id && r.RecipeTitle == f.RecipeTitle)
                .Select(r => r.Rating)
                .FirstOrDefaultAsync();

            var vm = new FavoriteRecipeViewModel
            {
                Id = f.Id,
                RecipeTitle = f.RecipeTitle,
                Ingredients = JsonSerializer.Deserialize<List<string>>(f.IngredientsJson) ?? new(),
                Instructions = JsonSerializer.Deserialize<List<string>>(f.InstructionsJson) ?? new(),
                FavoritedAt = f.FavoritedAt,
                Rating = rating == 0 ? (int?)null : rating
            };

            return View(vm);
        }

        // GET: /MealPlanner/Substitutions
        [HttpGet]
        public async Task<IActionResult> Substitutions(
            string ingredient,
            string ingredientsJson,
            bool vegetarian,
            bool glutenFree)
        {
            if (string.IsNullOrWhiteSpace(ingredient))
                return BadRequest();

            var subs = await _chat.SuggestSubstitutesAsync(
                ingredient, vegetarian, glutenFree);

            ViewBag.OriginalIngredient = ingredient;
            ViewBag.IngredientsJson = ingredientsJson;
            ViewBag.IsVegetarian = vegetarian;
            ViewBag.IsGlutenFree = glutenFree;

            return View(subs);
        }

        // POST: /MealPlanner/ReplaceIngredient
        [HttpPost]
        public IActionResult ReplaceIngredient(
            string original,
            string replacement,
            string ingredientsJson,
            bool vegetarian,
            bool glutenFree)
        {
            var list = JsonSerializer.Deserialize<List<string>>(ingredientsJson)
                       ?? new List<string>();

            var idx = list.FindIndex(x => x == original);
            if (idx >= 0) list[idx] = replacement;

            return RedirectToAction(nameof(Suggest), new
            {
                ingredients = list,
                vegetarian,
                glutenFree
            });
        }

        [HttpGet]
        public async Task<IActionResult> SubstitutesJson(
            string ingredient,
            bool vegetarian,
            bool glutenFree)
                {
                    if (string.IsNullOrWhiteSpace(ingredient))
                        return BadRequest("Missing ingredient");

                    var subs = await _chat.SuggestSubstitutesAsync(
                        ingredient, vegetarian, glutenFree);

                    return Json(subs);
                }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? throw new InvalidOperationException("User not found");

            var history = await _db.UserHistories
                .Where(h => h.UserEmail == user.Email)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            return View(history);
        }

        [HttpGet]
        public async Task<IActionResult> LoadFromHistory(int id)
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? throw new InvalidOperationException("User not found");

            var history = await _db.UserHistories
                .Where(h => h.Id == id && h.UserEmail == user.Email)
                .FirstOrDefaultAsync();

            if (history == null) return NotFound();

            var ingredients = JsonSerializer.Deserialize<List<string>>(history.IngredientsJson) ?? new();

            TempData["IngredientsJson"] = JsonSerializer.Serialize(ingredients);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? throw new InvalidOperationException("User not found");

            var history = await _db.UserHistories
                .FirstOrDefaultAsync(h => h.Id == id && h.UserEmail == user.Email);

            if (history != null)
            {
                _db.UserHistories.Remove(history);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(History));
        }


    }
}
