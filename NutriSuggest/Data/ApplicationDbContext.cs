// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NutriSuggest.Models;

namespace NutriSuggest.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts)
            : base(opts) { }

        public DbSet<RecipeRating> RecipeRatings { get; set; }
        public DbSet<FavoriteRecipe> FavoriteRecipes { get; set; }

        public DbSet<UserHistory> UserHistories { get; set; }
    }
}
