using FoodRecipe.Entity;
using Microsoft.EntityFrameworkCore;
using FoodRecipe.Enums;

namespace FoodRecipe.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<UserLogin> UserLogins => Set<UserLogin>();
        public DbSet<EmailCode> EmailCodes => Set<EmailCode>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredients> Ingredients { get; set; }
        public DbSet<RecipeSteps> RecipeSteps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("user");
                e.Property(u => u.Role).HasConversion<string>();
                e.Property(u => u.Email).HasMaxLength(254);
                e.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<UserLogin>(e =>
            {
                e.ToTable("user_login");
                e.Property(l => l.Provider).HasMaxLength(32);
                e.Property(l => l.ProviderKey).HasMaxLength(256);
                e.HasIndex(l => new { l.Provider, l.ProviderKey }).IsUnique();
                e.HasOne(l => l.User)
                    .WithMany()
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EmailCode>(e =>
            {
                e.ToTable("email_code");
                e.Property(c => c.Email).HasMaxLength(254);
                e.Property(c => c.CodeHash).HasMaxLength(128);
                e.HasIndex(c => c.Email);
            });
        }
    }
}