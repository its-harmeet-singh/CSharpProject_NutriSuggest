using Microsoft.EntityFrameworkCore;
using NutriSuggest.Data;
using NutriSuggest.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. EF Core DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. ChatGPT wrapper
builder.Services.AddSingleton<ChatGptService>();

// 3. MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5. No authentication/authorization

// 6. Map default route to MealPlannerController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MealPlanner}/{action=Index}/{id?}");

app.Run();
