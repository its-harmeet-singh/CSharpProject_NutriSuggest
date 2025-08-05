using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NutriSuggest.Data;
using NutriSuggest.Models;

namespace NutriSuggest.Controllers
{
    [Authorize]
    public class CookBookController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CookBookController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var recipes = _db.CookBookRecipes
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            return View(recipes);
        }

        [HttpPost]
        public async Task<IActionResult> Add(string title, string ingredients, string? instructions)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (string.IsNullOrEmpty(user.Email))
                return BadRequest("User email is missing.");

            var recipe = new CookBookRecipe
            {
                Title = title,
                Ingredients = ingredients,
                Instructions = instructions,
                AuthorEmail = user!.Email
            };

            _db.CookBookRecipes.Add(recipe);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
