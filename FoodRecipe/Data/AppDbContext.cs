using FoodRecipe.Entity;
using Microsoft.EntityFrameworkCore;

namespace FoodRecipe.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
    }
}
