namespace FoodRecipe.Entity
{
    public class Recipe : BaseEntity
    {
        public Guid AuthorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; } = null;
        public int CookingTimeMinutes { get; set; }
        public List<Ingredients> Ingredients { get; set;} = new();
        public string UserId { get; set; } = default!;
        public List<RecipeSteps> Steps { get; set; } = new();
    }
}
