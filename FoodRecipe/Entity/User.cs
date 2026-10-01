using FoodRecipe.Enums;
using System.ComponentModel.DataAnnotations;

namespace FoodRecipe.Entity
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public UserRole Role { get; set; } = UserRole.User;
    }
}
