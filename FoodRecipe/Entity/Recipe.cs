namespace FoodRecipe.Entity
{
    public class Recipe : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; } = null;
        public int CookingTimeMinutes { get; set; }
    }
}
