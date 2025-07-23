using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NutriSuggest.Data;
using NutriSuggest.Models;
using NutriSuggest.Services;

var builder = WebApplication.CreateBuilder(args);

// EF Core + Identity
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(opts =>
{
    opts.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// ChatGPT wrapper
builder.Services.AddSingleton<ChatGptService>();

// MVC + Identity UI
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapControllerRoute(
  name: "default",
  pattern: "{controller=MealPlanner}/{action=Index}/{id?}");
app.MapRazorPages();  // for /Identity/Account/Login etc.

app.Run();
