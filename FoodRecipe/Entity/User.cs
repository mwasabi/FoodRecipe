using FoodRecipe.Enums;

namespace FoodRecipe.Entity
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Имя заполняется после первого входа (пока null)
        public string? Name { get; set; }

        public string Email { get; set; } = "";
        public bool EmailConfirmed { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
    // Привязка внешнего входа (Google) к пользователю
    public class UserLogin
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Provider { get; set; } = "";     // "Google"
        public string ProviderKey { get; set; } = "";  // id пользователя у провайдера (поле sub)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public class EmailCode
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = "";
        public string CodeHash { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public int Attempts { get; set; }
        public bool Used { get; set; }
    }
}