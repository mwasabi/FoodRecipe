using System.Globalization;

namespace FoodRecipe.Entity
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public List<Recipe> Recipes { get; set; } = new();
    }
}
