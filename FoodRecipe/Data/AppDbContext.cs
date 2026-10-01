using FoodRecipe.Entity;
using Microsoft.EntityFrameworkCore;
using FoodRecipe.Enums;

namespace FoodRecipe.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingridients> Ingridients { get; set; }
    }
}
