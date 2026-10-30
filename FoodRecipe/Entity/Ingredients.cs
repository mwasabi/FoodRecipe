namespace FoodRecipe.Entity
{
    public class Ingredients : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;   // кг, гр, шт, мл
        public Guid RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
    }
}
